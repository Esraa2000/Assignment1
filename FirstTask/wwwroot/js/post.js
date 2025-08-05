 const token = localStorage.getItem("token")




 let currentSlug = "";

 function loadAllPosts() {
     clearFilters();
     fetch('/api/posts')
         .then(res => res.json())
         .then(posts => displayPosts(posts));
 }

   function loadCategories() {
      
      fetch('/api/categories')
          .then(res => res.json())
          .then(categories => {
              const container = document.getElementById('categories');
              container.innerHTML = categories.map(cat =>
                  `<button class="btn custom-btn smoothscroll me-2 mb-2" onclick="filterByCategory('${encodeURIComponent(cat.name)}')">${cat.name}</button>`
              ).join('');
          });
  }

 function loadTags() {
     
     fetch('/api/tags')
         .then(res => res.json())
         .then(tags => {
             const container = document.getElementById('tags');
             container.innerHTML = tags.map(tag =>
                 `<button class="btn custom-btn smoothscroll me-2 mb-2" onclick="filterByTag('${tag.name}')">${tag.name}</button>`
             ).join('');
         });
 }

 function clearFilters() {
     document.getElementById('categories').innerHTML = '';
     document.getElementById('tags').innerHTML = '';
     document.getElementById('posts').innerHTML = '';
     document.getElementById('searchInput').value = '';
 }

 function displayPosts(posts) {
     const postsDiv = document.getElementById('posts');
     if (!posts || posts.length === 0) {
         postsDiv.innerHTML = '<p>No posts found.</p>';
         return;
     }
     postsDiv.innerHTML = posts.map(post =>
         `<article>
                                     <h3 style="cursor:pointer;" onclick="showPost('${post.slug || post.Slug}')">${post.title || post.Title}</h3>
                                     <p>${post.Content || post.Content || ''}</p>
                                     <small>Published: ${post.publishedDate || post.PublishedDate || ''}</small>
                                 </article>`
     ).join('');
 }

 
 async function createPost(event) {
     if (!token) {
         alert("❌ Unauthorized! Please login first.");
         window.location.href = "index.html";
     }
     event.preventDefault();
     const form = document.getElementById('createPostForm');
     const statusDiv = document.getElementById('createStatus');
     const formData = new FormData(form);
     const imageInput = document.getElementById('image');
     if (imageInput && imageInput.files.length > 0) {
         formData.append("image", imageInput.files[0]);
     }

     try {
         const response = await fetch(`/api/posts`, {
             method: 'POST',
             headers: {
                 "Authorization": `Bearer ${token}`
             }
               ,  body: formData
         });

         if (response.ok) {
             const result = await response.json();
             statusDiv.style.color = 'green';
             statusDiv.textContent = `✅ Post created successfully with slug: ${result.slug}`;
             form.reset();
             loadAllPosts();
         } else {
             const errorText = await response.text();
             statusDiv.style.color = 'red';
             statusDiv.textContent = '❌ Failed to create post: ' + errorText;
         }
     } catch (err) {
         statusDiv.style.color = 'red';
         statusDiv.textContent = '❌ Error: ' + err.message;
         console.error(err);
     }
 }

 function showPost(slug) {
     fetch(`/api/posts/${slug}`)
         .then(res => res.json())
         .then(post => {
             currentSlug = slug;
             document.getElementById('posts').style.display = 'none';
             document.getElementById('postDetails').style.display = 'block';
             document.getElementById('postTitle').textContent = post.title;

             const img = document.getElementById('postImage');
             if (post.image && post.image.trim() !== "") {
                 img.src = post.image;
                 img.style.display = 'block';
             } else {
                 img.style.display = 'none';
             }

             document.getElementById('postContent').innerHTML = post.content;
             const meta = `Published on ${post.publishedDate || ''} | Categories: ${(post.categories || []).join(', ')} | Tags: ${(post.tags || []).join(', ')}`;
             document.getElementById('postMeta').textContent = meta;
         })
         .catch(err => {
             alert("Error loading post");
             console.error(err);
         });
 }

 function goBack() {
     document.getElementById('postDetails').style.display = 'none';
     document.getElementById('posts').style.display = 'block';
     document.getElementById("editPostFormWrapper").style.display = "none";


 }

 function deletePost() {
     if (!token) {
         alert("❌ Unauthorized! Please login first.");
         window.location.href = "index.html";
     }
     if (confirm("Are you sure you want to delete this post?")) {
         fetch(`/api/posts/${currentSlug}`, {
             method: 'DELETE',
             headers: {
                 "Authorization": `Bearer ${token}` }
         })
             .then(response => {
                 if (response.ok ) {
                     alert("Post deleted.");
                     goBack();
                     loadAllPosts();
                 } else {
                     alert("Failed to delete post.");
                 }
             });
     }
 }

 function editPost() {
     if (!token) {
         alert("❌ Unauthorized! Please login first.");
         window.location.href = "index.html";
     }
     fetch(`/api/posts/${currentSlug}`, {
             method: 'GET',
             headers: {
                 "Authorization": `Bearer ${ token }` }
         })
         .then(res => res.json())
         .then(post => {
             document.getElementById('editPostFormWrapper').style.display = 'block';
             document.getElementById('editTitle').value = post.title || '';
             document.getElementById('editContent').value = post.content || '';
             document.getElementById('editCategory').value = (post.categories || [])[0] || '';
             document.getElementById('editTags').value = (post.tags || []).join(', ');
             document.getElementById('editPublished').checked = post.status === "published";
         });
 }

 async function submitEdit(event) {

     event.preventDefault();
     const form = document.getElementById("editPostForm");
     const statusDiv = document.getElementById("editStatus");

     const formData = new FormData();
     formData.append("title", document.getElementById("editTitle").value);
     formData.append("content", document.getElementById("editContent").value);
     formData.append("category", document.getElementById("editCategory").value);
     formData.append("tags", document.getElementById("editTags").value);
     formData.append("published", document.getElementById("editPublished").checked);

     const imageFile = document.getElementById("editImage").files[0];
     if (imageFile) {
         formData.append("image", imageFile);
     }

     try {
         const response = await fetch(`/api/posts/${currentSlug}`, {
             method: 'PUT',
             headers: {
                 "Authorization": `Bearer ${token}`
             },
             body: formData
         });

         if (response.ok) {
             const result = await response.json();
             statusDiv.style.color = "green";
             statusDiv.textContent = "✅ Post updated successfully.";
             loadAllPosts();
             goBack();
         } else {
             const errorText = await response.text();
             statusDiv.style.color = "red";
             statusDiv.textContent = "❌ Failed to update post: " + errorText;
         }
     } catch (err) {
         statusDiv.style.color = "red";
         statusDiv.textContent = "❌ Error: " + err.message;
         console.error(err);
     }
 }

 function goToIndex() {
     window.location.href = "index.html"
 }

  async function login(event) {
     event.preventDefault();
     const username = document.getElementById("loginUsername").value;
     const password = document.getElementById("loginPassword").value;

     const res = await fetch('/api/login', {
         method: 'POST',
         headers: { 'Content-Type': 'application/json' },
         body: JSON.stringify({ username, password })
     });

     const data = await res.json();
     const status = document.getElementById("loginStatus");

     if (res.ok && data.token) {
         localStorage.setItem("token", data.token);
         window.location.href = "index.html";
     } else {
         status.textContent = "❌ Login failed.";
     }
 }

 async function register(event) {
     event.preventDefault();
     const username = document.getElementById("registerUsername").value;
     const password = document.getElementById("registerPassword").value;

     const res = await fetch('/api/users/register', {
         method: 'POST',
         headers: { 'Content-Type': 'application/json' },
         body: JSON.stringify({ username, password })
     });

     const status = document.getElementById("registerStatus");
     const message = await res.text();
     status.textContent = res.ok ? "✅ Registered successfully." : "❌ " + message;
 }
    function logout() {
        localStorage.removeItem("token");
        window.location.href = "index.html";
    }

window.addEventListener('DOMContentLoaded',function ()  {
    loadCategories();
    loadAllPosts();
    loadTags();
});


const token = localStorage.getItem("token");

const params = new URLSearchParams(window.location.search);
let currentSlug = params.get("slug");

async function loadAllPosts() {
    const res = await fetch('/api/posts');
    const postsDiv = document.getElementById('posts');
    if (!res.ok) {
        postsDiv.innerHTML = '<p class="text-danger">Failed to load posts.</p>';
        return;
    }

    const posts = await res.json();
    postsDiv.innerHTML = posts.map(p => {
        const slug = p.slug;
        const excerpt = (p.content || "").substring(0, 100);

        return `
       <div class="col-lg-6 col-12">
         <div class="transparent-card mb-2 p-3">
           <h5 class="text-white">${p.title}</h5>
           <div id="excerpt-${slug}" class="text-light">${excerpt}...</div>
           <div id="full-${slug}" class="text-light d-none">${p.content}</div>
           <div class="mt-2">
             <button class="btn btn-outline-light btn-sm"
                     data-slug="${slug}" data-expanded="false"
                     onclick="toggleContent(this)">View more</button>
           </div>
         </div>
       </div>`;
    }).join('');
}

function toggleContent(btn) {
    const slug = btn.getAttribute('data-slug');
    const ex = document.getElementById(`excerpt-${slug}`);
    const full = document.getElementById(`full-${slug}`);
    const expanded = btn.getAttribute('data-expanded') === 'true';

    if (expanded) {
        full.classList.add('d-none');
        ex.classList.remove('d-none');
        btn.textContent = 'View more';
        btn.setAttribute('data-expanded', 'false');
    } else {
        full.classList.remove('d-none');
        ex.classList.add('d-none');
        btn.textContent = 'View less';
        btn.setAttribute('data-expanded', 'true');
    }
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
}

function filterByCategory(name) {
    fetch(`/api/posts/category/${encodeURIComponent(name)}`)
        .then(r => r.ok ? r.json() : [])
        .then(posts => {
            displayPosts(posts);
        })
        .catch(console.error);
}

function filterByTag(name) {
    fetch(`/api/posts/tag/${encodeURIComponent(name)}`)
        .then(r => r.ok ? r.json() : [])
        .then(posts => {
            displayPosts(posts);
        })
        .catch(console.error);
}
function filterAdminByCategory(name) {
    const token = localStorage.getItem("token");
    fetch(`/api/posts/category/${encodeURIComponent(name)}`, {
        headers: { Authorization: `Bearer ${token}` }
    })
        .then(r => r.ok ? r.json() : [])
        .then(posts => {
            const postsDiv = document.getElementById('adminPosts');
            if (!posts || posts.length === 0) {
                postsDiv.innerHTML = '<p class="text-white">No posts found.</p>';
                return;
            }
            postsDiv.innerHTML = posts.map(p => {
                const slug = p.slug;
                const excerpt = (p.content || "").substring(0, 100);
                return `
            <div class="col-lg-6 col-12">
              <div class="transparent-card mb-2">
                <h5>${p.title}</h5>
                <p id="excerpt-${slug}" class="excerpt">${excerpt}...</p>
                <div id="full-${slug}" class="full d-none">${p.content}</div>
                <div class="mt-2 d-flex gap-2">
                  <button class="btn btn-sm btn-primary" onclick="goEditPost('${slug}')">Edit</button>
                  <button class="btn btn-sm btn-danger" onclick="confirmDelete('${slug}')">Delete</button>
                  <button class="btn btn-outline-light btn-sm" data-slug="${slug}" data-expanded="false" onclick="toggleContent(this)">View more</button>
                </div>
              </div>
            </div>`;
            }).join('');
        })
        .catch(console.error);
}
function filterAdminByTag(name) {
    const token = localStorage.getItem("token");
    fetch(`/api/posts/tag/${encodeURIComponent(name)}`, {
        headers: { Authorization: `Bearer ${token}` }
    })
        .then(r => r.ok ? r.json() : [])
        .then(posts => {
            const postsDiv = document.getElementById('adminPosts');
            if (!posts || posts.length === 0) {
                postsDiv.innerHTML = '<p class="text-white">No posts found.</p>';
                return;
            }
            postsDiv.innerHTML = posts.map(p => {
                const slug = p.slug;
                const excerpt = (p.content || "").substring(0, 100);
                return `
            <div class="col-lg-6 col-12">
              <div class="transparent-card mb-2">
                <h5>${p.title}</h5>
                <p id="excerpt-${slug}" class="excerpt">${excerpt}...</p>
                <div id="full-${slug}" class="full d-none">${p.content}</div>
                <div class="mt-2 d-flex gap-2">
                  <button class="btn btn-sm btn-primary" onclick="goEditPost('${slug}')">Edit</button>
                  <button class="btn btn-sm btn-danger" onclick="confirmDelete('${slug}')">Delete</button>
                  <button class="btn btn-outline-light btn-sm" data-slug="${slug}" data-expanded="false" onclick="toggleContent(this)">View more</button>
                </div>
              </div>
            </div>`;
            }).join('');
        })
        .catch(console.error);
}
function displayPosts(posts) {
    const postsDiv = document.getElementById('posts');
    if (!posts || posts.length === 0) {
        postsDiv.innerHTML = '<p class="text-white">No posts found.</p>';
        return;
    }

    postsDiv.innerHTML = posts.map(p => {
        const slug = p.slug || p.Slug;

        return `
        <div class="col-12 d-flex justify-content-center">
            <div  style="max-width:600px; text-align:center;">
                <h5 class="text-white">${p.title || p.Title}</h5>
                <div class="text-light">${p.content || p.Content}</div>
            </div>
        </div>`;
    }).join('');
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
            headers: { "Authorization": `Bearer ${token}` },
            body: formData
        });

        if (response.ok) {
            const result = await response.json();
            statusDiv.style.color = 'green';
            statusDiv.textContent = `✅ Post created successfully with slug: ${result.slug}`;
            form.reset();
            loadAllPosts();
            loadCategories(); 
            loadTags(); 
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



async function deletePost(slug) {
    const token = localStorage.getItem("token");
    if (!token) {
        alert("❌ Unauthorized! Please login first.");
        return window.location.href = "index.html";
    }

    if (confirm("Are you sure you want to delete this post?")) {
        const res = await fetch(`/api/posts/${slug}`, {
            method: 'DELETE',
            headers: { "Authorization": `Bearer ${token}` }
        });

        if (res.ok) {
            alert("✅ Post deleted.");
            loadAdminPosts();
            loadCategories(); 
            loadTags();     
        } else {
            alert("❌ Failed to delete post.");
        }
    }
}

function editPost() {
    if (!token) {
        alert("❌ Unauthorized! Please login first.");
        window.location.href = "index.html";
    }
}


async function loadPost() {
    if (!currentSlug) return;

    try {
        const res = await fetch(`/api/posts/${currentSlug}`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        if (!res.ok) throw new Error("Failed to load post");

        const post = await res.json();

        document.getElementById("editTitle").value = post.title || "";
        document.getElementById("editContent").value = post.content || "";
        document.getElementById("editCategory").value = (post.categories && post.categories.length > 0) ? post.categories[0] : "";
        document.getElementById("editTags").value = (post.tags && post.tags.length > 0) ? post.tags.join(", ") : "";
        document.getElementById("editPublished").checked = (post.status === "published");

    } catch (err) {
        alert("Error loading post: " + err.message);
    }
}

async function submitEdit(event) {
    event.preventDefault();
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
            method: "PUT",
            headers: { "Authorization": `Bearer ${token}` },
            body: formData
        });

        if (response.ok) {
            statusDiv.style.color = "lightgreen";
            statusDiv.textContent = "✅ Post updated successfully.";
            setTimeout(() => {
                window.location.href = "dashboard.html";
            }, 1500);
        } else {
            const errorText = await response.text();
            statusDiv.style.color = "red";
            statusDiv.textContent = "❌ Failed: " + errorText;
        }
    } catch (err) {
        statusDiv.style.color = "red";
        statusDiv.textContent = "❌ Error: " + err.message;
    }
}


function loadAdminCategories() {
    fetch('/api/categories')
        .then(res => res.json())
        .then(categories => {
            const container = document.getElementById("adminCategories");
            container.innerHTML = categories.map(cat => `
                <div class="col-lg-4 col-md-6 col-12 mb-2">
                    <button class="btn custom-btn w-100" onclick="loadAdminPostsByCategory('${cat.id}')">
                        ${cat.name}
                    </button>
                </div>
            `).join('');
        });
}

// Load tags
function loadAdminTags() {
    fetch('/api/tags')
        .then(res => res.json())
        .then(tags => {
            const container = document.getElementById("adminTags");
            container.innerHTML = tags.map(tag => `
                <div class="col-lg-4 col-md-6 col-12 mb-2">
                    <button class="btn custom-btn w-100" onclick="loadAdminPostsByTag('${tag.id}')">
                        ${tag.name}
                    </button>
                </div>
            `).join('');
        });
}


function loadAdminPostsByCategory(categoryId) {
    fetch(`/api/categories/${categoryId}/posts`)
        .then(res => res.json())
        .then(posts => renderAdminPosts(posts));
}

// Load posts by tag
function loadAdminPostsByTag(tagId) {
    fetch(`/api/tags/${tagId}/posts`)
        .then(res => res.json())
        .then(posts => renderAdminPosts(posts));
}

// Render posts in dashboard
function renderAdminPosts(posts) {
    const container = document.getElementById("adminPosts");
    container.innerHTML = "";
    if (posts.length === 0) {
        container.innerHTML = "<p class='text-white'>No posts found.</p>";
        return;
    }
    posts.forEach(post => {
        const div = document.createElement("div");
        div.className = "col-lg-6 col-md-12 col-12";
        div.innerHTML = `
            <div class="custom-block">
                <h5 class="mb-2 text-white">${post.title}</h5>
                <p class="text-white">${post.excerpt || ""}</p>
                <button class="btn btn-sm btn-light me-2" onclick="window.location='edit.html?slug=${post.slug}'">Edit</button>
                <button class="btn btn-sm btn-danger" onclick="deletePost('${post.slug}')">Delete</button>
            </div>`;
        container.appendChild(div);
    });
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
        window.location.href = "dashboard.html";
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

    if (res.ok) {
        window.location.href = "index.html";
    } else {
        const message = await res.text();
        alert("❌ " + message);
    }
}
async function loadAdminPosts() {
    const token = localStorage.getItem("token");
    if (!token) return window.location = "login.html";

    const res = await fetch('/api/posts', {
        headers: { Authorization: `Bearer ${token}` }
    });

    const postsDiv = document.getElementById('adminPosts');
    if (!res.ok) return postsDiv.innerHTML = '<p class="text-danger">Failed to load posts.</p>';

    const posts = await res.json();
    postsDiv.innerHTML = posts.map(p => {
        const slug = p.slug;
        const excerpt = (p.content || "").substring(0, 100);

        return `
                <div class="col-lg-6 col-12">
                  <div class="transparent-card mb-2">
                    <h5>${p.title}</h5>

                    <p id="excerpt-${slug}" class="excerpt">${excerpt}...</p>
                    <div id="full-${slug}" class="full d-none">${p.content}</div>

                    <div class="mt-2 d-flex gap-2">
                      <button class="btn btn-sm btn-primary" onclick="goEditPost('${slug}')">Edit</button>
                      <button class="btn btn-sm btn-danger" onclick="confirmDelete('${slug}')">Delete</button>
                      <button class="btn btn-outline-light btn-sm"
                              data-slug="${slug}" data-expanded="false"
                              onclick="toggleContent(this)">View more</button>
                    </div>
                  </div>
                </div>`;
    }).join('');
}


function goEditPost(slug) { window.location = `edit.html?slug=${slug}`; }
function confirmDelete(slug) {
    if (confirm("Are you sure to delete this post?")) {
        deletePost(slug).then(loadAdminPosts);
    }
}

function logout() {
    localStorage.removeItem("token");
    window.location.href = "index.html";
}
document.addEventListener("DOMContentLoaded", function () {
    loadCategories();
    loadTags();
    loadAllPosts();
    if (currentSlug) {
        loadPost();
    }
});

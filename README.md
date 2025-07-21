# File-Based Blog Management System

This project is a lightweight blogging platform built using **ASP.NET Core 10 Minimal API**, focused on simplicity and efficiency by managing all content using the file system (no database). The blog content, users, categories, and tags are stored using structured folders with JSON and Markdown files.

---

✅ Implemented Features

1. Blog Post Management
- Create, edit, and delete blog posts.
- Each post is saved inside its own folder:  
  `/content/posts/{slug}/`
  - `meta.json`: Contains title, description, slug, category, tags, status (published/draft), etc.
  - `content.md`: Contains the Markdown content of the post.
  - `assets/`: Folder for storing images and files related to the post.

2. Markdown Rendering
- Blog content is written in Markdown and converted to HTML using **Markdig** for frontend display.

3. Image Upload and Auto Resize
- Images can be uploaded while creating or editing a post.
- Images are resized automatically to max width 1024px using **ImageSharp**.
- Stored under: `/content/posts/{slug}/assets/`.

4. Categories and Tags
- Posts are categorized and tagged.
- Categories and tags are stored as individual JSON files:
  - `/content/categories/{category-name}.json`
  - `/content/tags/{tag-name}.json`
- When deleting a post, its unused tags/categories are also deleted if no other posts use them.

 5. Post Search and Filtering
- Filter posts by:
  - Category: `/api/posts/category/{categoryName}`
  - Tag: `/api/posts/tag/{tagName}`
 *- Search functionality for posts by title or description. 

🗂️ Project Folder Structure

/Content
├── /posts
│   └── /{slug}/
│       ├── meta.json       # Stores post metadata (title, description, categories, tags, status)
│       ├── content.md      # Markdown content of the post
│       └── /assets/        # Images and files related to the post
├── /categories
│   └── {category-name}.json  # Metadata for each category
├── /tags
│   └── {tag-name}.json       # Metadata for each tag

Technologies Used

- **ASP.NET Core 10 Minimal API**
- **Markdig** for Markdown to HTML conversion
- **ImageSharp** for image processing and resizing
- **System.Text.Json** for data storage and parsing 

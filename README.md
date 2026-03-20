Photo Search — Semantic Image Search with CLIP
A full-stack application for semantic image search using AI. Built with ASP.NET Web API and Angular.
How it works
Images are indexed at startup using the CLIP (Contrastive Language-Image Pretraining) model by OpenAI. Each image is converted into a 512-dimensional vector and stored in memory. When a user types a search query, the text is converted into a vector using the same model and compared against all image vectors using cosine similarity. The most relevant images are returned ranked by score.
Tech Stack
Backend

ASP.NET Web API (.NET 8)
ONNX Runtime — local inference without external API calls
CLIP ViT-B/32 — image and text encoding
SixLabors.ImageSharp — image preprocessing

Frontend

Angular 20
Proxy configuration for seamless API communication

Features

Semantic search in English — finds images by meaning, not filename
Automatic indexing of all images at startup
Real-time indexing status
Results ranked by similarity score

Getting Started

Clone the repository
Place images in the Images/ folder
Download CLIP ONNX models and place them in PhotoSearch.Api/Models/clip/
Run the API: dotnet run
Run the client: ng serve
Open http://localhost:4200


Note: CLIP model files (*.onnx, *.onnx.data) are not included in the repository due to their size (~600MB). See the instructions below for generating them.
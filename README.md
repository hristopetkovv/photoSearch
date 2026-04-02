Photo Search — Semantic Image Search with CLIP
A full-stack application for semantic image search using AI. Built with ASP.NET Web API and Angular.
How it works - images are indexed at startup using the CLIP (Contrastive Language-Image Pretraining) model by OpenAI. Each image is converted into a 512-dimensional vector and stored either in-memory or in PostgreSQL with pgvector. When a user types a search query in Bulgarian or English, the text is automatically translated to English, converted into a vector using the same model and compared against all image vectors using cosine similarity. The most relevant images are returned ranked by score.

## Tech Stack

### Backend

- ASP.NET Web API (.NET 9)

- ONNX Runtime — local inference without external API calls

- CLIP ViT-B/32 — image and text encoding

- SixLabors.ImageSharp — image preprocessing

- PostgreSQL + pgvector — vector storage and similarity search

- LibreTranslate — local Bulgarian to English translation

### Frontend

- Angular 20

- ngx-toastr — notifications

## Features

- Semantic search in Bulgarian and English — finds images by meaning, not filename

- Automatic indexing of all images at startup

- Skips already indexed images on restart

- Upload new images without restarting the application

- Real-time indexing status

- Results ranked by similarity score

- Supports both in-memory and PostgreSQL vector storage

## Prerequisites

- .NET 9 SDK

- Node.js and Angular CLI

- Python 3.x — only for generating CLIP ONNX models (one-time setup)

- Docker — for PostgreSQL with pgvector (optional, only if using PostgreSQL storage)

- LibreTranslate — for Bulgarian language support

## Notes

> CLIP model files (`*.onnx`, `*.onnx.data`) are not included in the repository due to their size (~600MB)

> LibreTranslate must be running for Bulgarian language search to work. English queries work without it.
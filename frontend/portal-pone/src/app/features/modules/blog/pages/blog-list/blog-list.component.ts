import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { BlogService, BlogPostDto, CreateBlogPostCommand, UpdateBlogPostCommand } from '../../services/blog.service';
import { QuillModule } from 'ngx-quill';

@Component({
  selector: 'app-blog-list',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, QuillModule],
  templateUrl: './blog-list.component.html',
  styleUrl: './blog-list.component.scss'
})
export class BlogListComponent implements OnInit {
  posts: BlogPostDto[] = [];
  isLoading = true;

  // Modal State
  showFormModal = false;
  isEditMode = false;
  postId?: number;
  isSaving = false;
  isUploading = false;
  errorMessage = '';

  selectedFile: File | null = null;
  imagePreviewUrl: string | null = null;

  quillModules = {
    toolbar: [
      ['bold', 'italic', 'underline', 'strike'],
      ['blockquote'],
      [{ 'header': 1 }, { 'header': 2 }],
      [{ 'list': 'ordered'}, { 'list': 'bullet' }],
      [{ 'script': 'sub'}, { 'script': 'super' }],
      [{ 'indent': '-1'}, { 'indent': '+1' }],
      [{ 'size': ['small', false, 'large', 'huge'] }],
      [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
      [{ 'color': [] }, { 'background': [] }],
      [{ 'align': [] }],
      ['clean'],
      ['link', 'video'] // NO 'image' here
    ]
  };

  blogForm!: FormGroup;

  @ViewChild('fileInput') fileInput!: ElementRef;

  constructor(
    private blogService: BlogService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadPosts();
  }

  initForm(): void {
    this.blogForm = this.fb.group({
      title: ['', Validators.required],
      subject: ['', Validators.required],
      content: ['', Validators.required],
      tags: [''],
      headerImageUrl: ['']
    });
  }

  loadPosts(): void {
    this.isLoading = true;
    this.blogService.getAll().subscribe({
      next: (res) => {
        this.posts = res || [];
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Erro ao carregar posts', err);
        this.isLoading = false;
      }
    });
  }

  deletePost(id: number): void {
    if (confirm('Tem certeza que deseja excluir esta publicação? Esta ação não pode ser desfeita.')) {
      this.blogService.delete(id).subscribe({
        next: () => {
          this.loadPosts();
        },
        error: (err) => {
          console.error('Erro ao excluir post', err);
        }
      });
    }
  }

  openNewPostModal(): void {
    this.isEditMode = false;
    this.postId = undefined;
    this.errorMessage = '';
    this.selectedFile = null;
    this.imagePreviewUrl = null;
    this.blogForm.reset();
    this.showFormModal = true;
  }

  openEditPostModal(post: BlogPostDto): void {
    this.isEditMode = true;
    this.postId = post.id;
    this.errorMessage = '';
    this.selectedFile = null;
    this.imagePreviewUrl = post.headerImageUrl || null;
    this.blogForm.patchValue({
      title: post.title,
      subject: post.subject,
      content: post.content,
      tags: post.tags,
      headerImageUrl: post.headerImageUrl
    });
    this.showFormModal = true;
  }

  closeModal(): void {
    this.showFormModal = false;
    this.selectedFile = null;
    this.imagePreviewUrl = null;
  }

  triggerFileInput(): void {
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      if (file.size > 1.5 * 1024 * 1024) {
        this.errorMessage = 'A imagem selecionada excede o limite de 1.5MB.';
        this.fileInput.nativeElement.value = '';
        return;
      }

      this.errorMessage = '';
      this.selectedFile = file;
      this.imagePreviewUrl = URL.createObjectURL(file);
    }
  }

  removeImage(event: Event): void {
    event.stopPropagation();
    this.selectedFile = null;
    this.imagePreviewUrl = null;
    this.blogForm.patchValue({ headerImageUrl: '' });
    if (this.fileInput && this.fileInput.nativeElement) {
      this.fileInput.nativeElement.value = '';
    }
  }

  onSubmit(): void {
    if (this.blogForm.invalid) {
      this.blogForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    if (this.selectedFile) {
      this.isUploading = true;
      this.blogService.uploadImage(this.selectedFile).subscribe({
        next: (res) => {
          this.blogForm.patchValue({ headerImageUrl: res.url });
          this.isUploading = false;
          this.savePost();
        },
        error: (err) => {
          console.error('Erro ao fazer upload da imagem', err);
          this.errorMessage = 'Erro ao fazer upload da imagem.';
          this.isUploading = false;
          this.isSaving = false;
        }
      });
    } else {
      this.savePost();
    }
  }

  private savePost(): void {
    const formValue = this.blogForm.value;

    if (this.isEditMode && this.postId) {
      const command: UpdateBlogPostCommand = {
        id: this.postId,
        ...formValue
      };
      
      this.blogService.update(this.postId, command).subscribe({
        next: () => {
          this.isSaving = false;
          this.closeModal();
          this.loadPosts();
        },
        error: (err) => {
          console.error('Erro ao atualizar post', err);
          this.errorMessage = 'Ocorreu um erro ao atualizar a publicação.';
          this.isSaving = false;
        }
      });
    } else {
      const command: CreateBlogPostCommand = {
        ...formValue
      };

      this.blogService.create(command).subscribe({
        next: () => {
          this.isSaving = false;
          this.closeModal();
          this.loadPosts();
        },
        error: (err) => {
          console.error('Erro ao criar post', err);
          this.errorMessage = 'Ocorreu um erro ao salvar a publicação.';
          this.isSaving = false;
        }
      });
    }
  }
}

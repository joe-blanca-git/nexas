import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CertificateService, MyCertificateItemDto } from '../../services/certificate.service';

@Component({
  selector: 'app-certificates-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './certificates-home.component.html',
  styleUrl: './certificates-home.component.scss'
})
export class CertificatesHomeComponent implements OnInit {
  searchTerm: string = '';
  certificates: MyCertificateItemDto[] = [];
  
  apiTotalCertificates: number = 0;
  apiTotalHours: number = 0;
  isLoading: boolean = true;

  private certService = inject(CertificateService);
  private router = inject(Router);

  async ngOnInit(): Promise<void> {
    try {
      this.isLoading = true;
      const data = await this.certService.getMyCertificates();
      this.certificates = data.certificates;
      this.apiTotalCertificates = data.totalCertificates;
      this.apiTotalHours = data.totalHours;
    } catch (error) {
      console.error('Erro ao buscar certificados', error);
    } finally {
      this.isLoading = false;
    }
  }

  get filteredCertificates(): MyCertificateItemDto[] {
    if (!this.searchTerm) {
      return this.certificates;
    }
    const term = this.searchTerm.toLowerCase();
    return this.certificates.filter(cert => 
      cert.courseName.toLowerCase().includes(term) ||
      cert.hash.toLowerCase().includes(term)
    );
  }

  get totalCertificates(): number {
    return this.apiTotalCertificates;
  }

  get totalHours(): number {
    return this.apiTotalHours;
  }

  viewCertificate(hash: string): void {
    this.router.navigate(['/certificates/viewer', hash]);
  }

  downloadPdf(hash: string): void {
    this.router.navigate(['/certificates/viewer', hash], { queryParams: { download: 'true' } });
  }
}

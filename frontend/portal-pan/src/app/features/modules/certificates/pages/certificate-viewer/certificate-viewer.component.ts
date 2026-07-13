import { Component, OnInit } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { CertificateService, CertificateDetailDto } from '../../services/certificate.service';
import html2canvas from 'html2canvas';
import { jsPDF } from 'jspdf';

@Component({
  selector: 'app-certificate-viewer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './certificate-viewer.component.html',
  styleUrls: ['./certificate-viewer.component.scss']
})
export class CertificateViewerComponent implements OnInit {
  certData: CertificateDetailDto | null = null;
  isLoading: boolean = true;
  isDownloading: boolean = false;
  errorMessage: string | null = null;
  validationUrl: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private certService: CertificateService,
    private location: Location
  ) {}

  async ngOnInit(): Promise<void> {
    const code = this.route.snapshot.paramMap.get('code');
    if (!code) {
      window.location.href = 'https://joederblanca.com.br/nexas';
      return;
    }

    this.validationUrl = window.location.href;

    try {
      this.certData = await this.certService.validateCertificate(code);
      if (this.certData && this.certData.issuedAt) {
        // Formatação manual garantida para evitar problemas de parse do DatePipe ou navegadores
        const datePart = this.certData.issuedAt.toString().split('T')[0];
        const [year, month, day] = datePart.split('-');
        this.certData.issuedAt = `${day}/${month}/${year}`;
      }
    } catch (error: any) {
      this.errorMessage = error.message || 'Erro ao validar o certificado.';
    } finally {
      this.isLoading = false;
      const shouldDownload = this.route.snapshot.queryParamMap.get('download') === 'true';
      if (shouldDownload && !this.errorMessage) {
        setTimeout(() => this.downloadPDF(), 800); // Dá tempo para as imagens e fontes renderizarem
      }
    }
  }

  goBack(): void {
    this.location.back();
  }

  async downloadPDF(): Promise<void> {
    if (!this.certData || this.isDownloading) return;

    this.isDownloading = true;
    try {
      const element = document.getElementById('certificate-content');
      if (!element) throw new Error('Elemento do certificado não encontrado');

      const canvas = await html2canvas(element, { scale: 2, useCORS: true });
      const imgData = canvas.toDataURL('image/png');

      const pdf = new jsPDF('l', 'mm', 'a4');
      const pdfWidth = pdf.internal.pageSize.getWidth();
      const pdfHeight = pdf.internal.pageSize.getHeight();

      pdf.addImage(imgData, 'PNG', 0, 0, pdfWidth, pdfHeight);
      pdf.save(`Certificado-${this.certData.courseTitle.replace(/\s+/g, '-')}.pdf`);
    } catch (error) {
      console.error('Erro ao gerar PDF:', error);
      alert('Erro ao gerar o PDF do certificado.');
    } finally {
      this.isDownloading = false;
    }
  }
}

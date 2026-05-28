import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface Certificate {
  id: string;
  hash: string;
  courseName: string;
  workload: number;
  completionDate: string;
  status: string;
}

@Component({
  selector: 'app-certificates-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './certificates-home.component.html',
  styleUrl: './certificates-home.component.scss'
})
export class CertificatesHomeComponent implements OnInit {
  searchTerm: string = '';
  certificates: Certificate[] = [];

  ngOnInit(): void {
    // Mock data for certificates
    this.certificates = [
      {
        id: 'cert-001',
        hash: 'A8F9-2B3C-4D5E-6F7G',
        courseName: 'Desenvolvimento Web com Angular',
        workload: 40,
        completionDate: '15/05/2026',
        status: 'Concluído'
      },
      {
        id: 'cert-002',
        hash: 'B1C2-3D4E-5F6G-7H8I',
        courseName: 'Arquitetura de Softwares Cloud',
        workload: 32,
        completionDate: '10/04/2026',
        status: 'Concluído'
      },
      {
        id: 'cert-003',
        hash: 'X9Y8-Z7W6-V5U4-T3S2',
        courseName: 'Lógica de Programação e Algoritmos',
        workload: 60,
        completionDate: '20/12/2025',
        status: 'Concluído'
      },
      {
        id: 'cert-004',
        hash: 'M1N2-O3P4-Q5R6-S7T8',
        courseName: 'Banco de Dados Relacionais e NoSQL',
        workload: 45,
        completionDate: '05/03/2026',
        status: 'Concluído'
      }
    ];
  }

  get filteredCertificates(): Certificate[] {
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
    return this.filteredCertificates.length;
  }

  get totalHours(): number {
    return this.filteredCertificates.reduce((acc, cert) => acc + cert.workload, 0);
  }

  viewCertificate(hash: string): void {
    // Simulating viewing action
    alert(`Visualizando certificado: ${hash}`);
  }

  downloadPdf(hash: string): void {
    // Simulating download action
    alert(`Iniciando download do PDF: ${hash}`);
  }
}

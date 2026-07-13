import { Injectable, Injector } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BaseService } from '../../../../core/services/base.service';
import { firstValueFrom } from 'rxjs';

export interface CertificateDetailDto {
  studentName: string;
  courseTitle: string;
  teacherName: string;
  workloadHours: number;
  validationCode: string;
  issuedAt: string | Date;
}

export interface MyCertificateItemDto {
  hash: string;
  courseName: string;
  workload: number;
  completionDate: string;
  status: string;
  coverImage?: string;
}

export interface MyCertificatesResponseDto {
  totalCertificates: number;
  totalHours: number;
  certificates: MyCertificateItemDto[];
}

@Injectable({
  providedIn: 'root'
})
export class CertificateService extends BaseService {
  
  constructor(injector: Injector, private http: HttpClient) {
    super(injector);
  }

  public async generateCertificate(courseId: number): Promise<string> {
    try {
      const response = await firstValueFrom(
        this.http.post<any>(
          `${this.urlApiNexas}certificates/generate`, 
          { courseId }, 
          this.GetAuthHeaderJson()
        )
      );
      return response.validationCode;
    } catch (error) {
      throw new Error('Falha ao gerar certificado.');
    }
  }

  public async validateCertificate(validationCode: string): Promise<CertificateDetailDto> {
    try {
      return await firstValueFrom(
        this.http.get<CertificateDetailDto>(
          `${this.urlApiNexas}certificates/validate/${validationCode}`
        )
      );
    } catch (error) {
      throw new Error('Certificado inválido ou não encontrado.');
    }
  }

  public async getMyCertificates(): Promise<MyCertificatesResponseDto> {
    try {
      return await firstValueFrom(
        this.http.get<MyCertificatesResponseDto>(
          `${this.urlApiNexas}certificates/my`,
          this.GetAuthHeaderJson()
        )
      );
    } catch (error) {
      throw new Error('Falha ao carregar certificados.');
    }
  }
}

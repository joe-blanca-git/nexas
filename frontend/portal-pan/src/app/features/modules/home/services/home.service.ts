import { HttpClient } from '@angular/common/http';
import { Injectable, Injector } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { BaseService } from '../../../../core/services/base.service';
import { IPortalHomeData } from '../models/home.model';

@Injectable({
  providedIn: 'root'
})
export class HomeService extends BaseService {
  constructor(injector: Injector, private httpClient: HttpClient) {
    super(injector);
  }

  async getHomeData(): Promise<IPortalHomeData> {
    try {
      let url = `${this.urlApiNexas}v1/portal/home`;
      const response = await firstValueFrom(
        this.httpClient.get<IPortalHomeData>(url, this.GetAuthHeaderJson())
      );
      return this.extractData(response) as IPortalHomeData;
    } catch (error) {
      throw error;
    }
  }

  async getNewsDetail(id: number): Promise<any> {
    try {
      let url = `${this.urlApiNexas}v1/portal/home/news/${id}`;
      const response = await firstValueFrom(
        this.httpClient.get<any>(url, this.GetAuthHeaderJson())
      );
      return this.extractData(response);
    } catch (error) {
      throw error;
    }
  }
}

import { Pipe, PipeTransform } from '@angular/core';
import { environment } from '../../environments/environment';

@Pipe({
  name: 'imgUrl',
  standalone: true
})
export class ImgUrlPipe implements PipeTransform {
  transform(value: string | undefined): string {
    if (!value) return 'assets/placeholder.png';
    if (value.startsWith('http')) return value;
    
    // Remove o '/api' da URL base para pegar a raiz do servidor onde está a pasta images
    const baseUrl = environment.apiUrl.replace('/api', '');
    return `${baseUrl}${value}`;
  }
}
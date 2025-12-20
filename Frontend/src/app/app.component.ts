import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router'; // <--- Importante

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet], // <--- Adicione aqui
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'market-placer';
}
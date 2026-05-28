import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-courses',
  standalone: true,
  imports: [RouterOutlet],
  template: '<router-outlet />',
})
export class CoursesComponent {

}

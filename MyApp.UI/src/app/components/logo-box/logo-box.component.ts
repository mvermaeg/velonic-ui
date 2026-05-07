import { Component, Input } from '@angular/core'
import { RouterLink } from '@angular/router'

@Component({
  selector: 'app-logo-box',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './logo-box.component.html',
  styles: ``,
})
export class LogoBoxComponent {
  @Input() className: string = ''
  @Input() size: boolean = false
}

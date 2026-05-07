import { CommonModule } from '@angular/common'
import {
  Component,
  ContentChild,
  inject,
  Renderer2,
  type OnDestroy,
  type OnInit,
  type TemplateRef,
} from '@angular/core'
import { RouterModule } from '@angular/router'
import { credits, currentYear } from '@common/constants'

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [RouterModule, CommonModule],
  templateUrl: './auth-layout.component.html',
  styles: ``,
})
export class AuthLayoutComponent implements OnInit, OnDestroy {
  year = currentYear
  credits = credits
  private renderer = inject(Renderer2)

  @ContentChild('bottomLinks') bottomLinksTemplate!: TemplateRef<
    HTMLElement | HTMLElement[]
  >
  isAuthPage: boolean = false

  ngOnInit(): void {
    this.renderer.addClass(document.body, 'authentication-bg')
  }

  ngOnDestroy(): void {
    this.renderer.removeClass(document.body, 'authentication-bg')
  }
}

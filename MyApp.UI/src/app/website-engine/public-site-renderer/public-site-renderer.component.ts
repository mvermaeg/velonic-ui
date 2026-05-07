import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { ActivatedRoute } from '@angular/router'
import { HttpClient } from '@angular/common/http'
import { environment } from '../../../environments/environment'
import { ModernTemplateComponent } from '../../../templates/template-modern/modern-template.component'

@Component({
  selector: 'app-public-site-renderer',
  standalone: true,
  imports: [CommonModule, ModernTemplateComponent],
  templateUrl: './public-site-renderer.component.html',
})
export class PublicSiteRendererComponent implements OnInit {
  private route = inject(ActivatedRoute)
  private http = inject(HttpClient)

  site: any = null
  loading = false
  errorMessage = ''

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug')

    if (!slug) {
      this.errorMessage = 'Invalid website URL.'
      return
    }

    this.loadSite(slug)
  }

  loadSite(slug: string) {
    this.loading = true
    this.errorMessage = ''

    this.http.get<any>(`${environment.apiUrl}/public/sites/by-slug/${slug}`).subscribe({
      next: (res) => {
        this.site = res
        this.loading = false
      },
      error: () => {
        this.loading = false
        this.errorMessage = 'Website not found.'
      },
    })
  }
}


// import { CommonModule } from '@angular/common'
// import { Component, OnInit, inject } from '@angular/core'
// import { ActivatedRoute } from '@angular/router'
// import { HttpClient } from '@angular/common/http'
// import { environment } from '../../../environments/environment'

// @Component({
//   selector: 'app-public-site-renderer',
//   standalone: true,
//   imports: [CommonModule],
//   templateUrl: './public-site-renderer.component.html',
// })
// export class PublicSiteRendererComponent implements OnInit {
//   private route = inject(ActivatedRoute)
//   private http = inject(HttpClient)

//   site: any = null
//   loading = false
//   errorMessage = ''

//   ngOnInit(): void {
//     const slug = this.route.snapshot.paramMap.get('slug')

//     if (!slug) {
//       this.errorMessage = 'Invalid website URL.'
//       return
//     }

//     this.loadSite(slug)
//   }

//   loadSite(slug: string) {
//     this.loading = true

//     this.http.get<any>(`${environment.apiUrl}/public/sites/by-slug/${slug}`).subscribe({
//       next: (res) => {
//         this.site = res
//         this.loading = false
//       },
//       error: () => {
//         this.loading = false
//         this.errorMessage = 'Website not found.'
//       },
//     })
//   }
// }
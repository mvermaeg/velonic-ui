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
    this.route.paramMap.subscribe((params) => {
      const slug = params.get('slug')
      const pageSlug = params.get('pageSlug')
      const domain = params.get('domain')

      this.clearPixels()

      if (domain) {
        this.loadSiteByDomain(domain, pageSlug)
        return
      }

      if (slug) {
        this.loadSiteBySlug(slug, pageSlug)
        return
      }

const host = window.location.hostname

if (host === 'localhost' || host === '127.0.0.1') {
  window.location.href = '/dashboard'
  return
}

this.loadSiteByDomain(host, null)    })
  }

  loadSiteBySlug(slug: string, pageSlug: string | null) {
    this.loading = true
    this.errorMessage = ''

    this.http.get<any>(`${environment.apiUrl}/public/sites/by-slug/${slug}`).subscribe({
      next: (res) => {
        this.site = {
          ...res,
          activePageSlug: pageSlug || 'home',
        }

        this.loading = false

        setTimeout(() => {
          this.injectPixels(this.site)
        }, 300)
        this.trackPixelEvent(this.site, null, 'PageView')
      },
      
      error: () => {
        this.loading = false
        this.errorMessage = 'Website not found.'
      },
      
    })
  }

  loadSiteByDomain(domain: string, pageSlug: string | null) {
    this.loading = true
    this.errorMessage = ''

    this.http
      .get<any>(`${environment.apiUrl}/public/sites/by-domain?domain=${domain}`)
      .subscribe({
        next: (res) => {
          this.site = {
            ...res,
            activePageSlug: pageSlug || 'home',
          }

          this.loading = false

          setTimeout(() => {
            this.injectPixels(this.site)
          }, 300)
          this.trackPixelEvent(this.site, null, 'PageView')
        },
        error: () => {
          this.loading = false
          this.errorMessage = 'Website not found for this domain.'
        },
      })
  }

  clearPixels() {
    document.querySelectorAll('[data-site-pixel]').forEach((x) => x.remove())
  }


  trackPixelEvent(site: any, pixelId: number | null, eventName: string) {
  if (!site?.id) return

  const payload = {
    siteId: site.id,
    sitePixelId: pixelId,
    pageSlug: site.activePageSlug || 'home',
    eventName: eventName,
    url: window.location.href,
  }

  this.http
    .post(`${environment.apiUrl}/public/pixel-events/track`, payload)
    .subscribe({
      next: () => {},
      error: () => {},
    })
}

  injectPixels(site: any) {
    this.clearPixels()

    const pixels = site?.pixels || []
    const activePageSlug = site?.activePageSlug || 'home'

    pixels.forEach((pixel: any) => {
      if (pixel.fireOnPageSlug && pixel.fireOnPageSlug !== activePageSlug) {
        return
      }

      if (!pixel.pixelCode) return

      const wrapper = document.createElement('div')
      wrapper.innerHTML = pixel.pixelCode

      wrapper.querySelectorAll('script').forEach((script: any) => {
        const newScript = document.createElement('script')
        newScript.setAttribute('data-site-pixel', String(pixel.id))

        if (script.src) {
          newScript.src = script.src
          newScript.async = true
        } else {
          newScript.text = script.innerHTML
        }

        document.body.appendChild(newScript)
        this.trackPixelEvent(site, pixel.id, 'PixelFired')
      })

      const noScriptItems = wrapper.querySelectorAll('noscript')
      noScriptItems.forEach((item: any) => {
        const holder = document.createElement('div')
        holder.setAttribute('data-site-pixel', String(pixel.id))
        holder.innerHTML = item.innerHTML
        document.body.appendChild(holder)
      })
    })
  }
}

// import { CommonModule } from '@angular/common'
// import { Component, OnInit, inject } from '@angular/core'
// import { ActivatedRoute } from '@angular/router'
// import { HttpClient } from '@angular/common/http'
// import { environment } from '../../../environments/environment'
// import { ModernTemplateComponent } from '../../../templates/template-modern/modern-template.component'

// @Component({
//   selector: 'app-public-site-renderer',
//   standalone: true,
//   imports: [CommonModule, ModernTemplateComponent],
//   templateUrl: './public-site-renderer.component.html',
// })
// export class PublicSiteRendererComponent implements OnInit {
//   private route = inject(ActivatedRoute)
//   private http = inject(HttpClient)

//   site: any = null
//   loading = false
//   errorMessage = ''

//   ngOnInit(): void {
//     this.route.paramMap.subscribe((params) => {
//       const slug = params.get('slug')
//       const pageSlug = params.get('pageSlug')
//       const domain = params.get('domain')

//       if (domain) {
//         this.loadSiteByDomain(domain, pageSlug)
//         return
//       }

//       if (slug) {
//         this.loadSiteBySlug(slug, pageSlug)
//         return
//       }

//       this.errorMessage = 'Invalid website URL.'
//     })
//   }

//   loadSiteBySlug(slug: string, pageSlug: string | null) {
//     this.loading = true
//     this.errorMessage = ''

//     this.http.get<any>(`${environment.apiUrl}/public/sites/by-slug/${slug}`).subscribe({
//       next: (res) => {
//         this.site = {
//           ...res,
//           activePageSlug: pageSlug,
//         }
//         this.loading = false
//       },
//       error: () => {
//         this.loading = false
//         this.errorMessage = 'Website not found.'
//       },
//     })
//   }


//   injectPixels(site: any) {
//   const pixels = site?.pixels || []
//   const activePageSlug = site?.activePageSlug || 'home'

//   pixels.forEach((pixel: any) => {
//     if (pixel.fireOnPageSlug && pixel.fireOnPageSlug !== activePageSlug) {
//       return
//     }

//     if (!pixel.pixelCode) return

//     const wrapper = document.createElement('div')
//     wrapper.innerHTML = pixel.pixelCode

//     wrapper.querySelectorAll('script').forEach((script: any) => {
//       const newScript = document.createElement('script')

//       if (script.src) {
//         newScript.src = script.src
//       } else {
//         newScript.text = script.innerHTML
//       }

//       newScript.setAttribute('data-site-pixel', pixel.id)

//       document.body.appendChild(newScript)
//     })
//   })
// }






//   loadSiteByDomain(domain: string, pageSlug: string | null) {
//     this.loading = true
//     this.errorMessage = ''

//     this.http
//       .get<any>(`${environment.apiUrl}/public/sites/by-domain?domain=${domain}`)
//       .subscribe({
//         next: (res) => {
//           this.site = {
//             ...res,
//             activePageSlug: pageSlug,
//           }
//           this.loading = false
//         },
//         error: () => {
//           this.loading = false
//           this.errorMessage = 'Website not found for this domain.'
//         },
//       })
//   }
// }


// // import { CommonModule } from '@angular/common'
// // import { Component, OnInit, inject } from '@angular/core'
// // import { ActivatedRoute } from '@angular/router'
// // import { HttpClient } from '@angular/common/http'
// // import { environment } from '../../../environments/environment'
// // import { ModernTemplateComponent } from '../../../templates/template-modern/modern-template.component'

// // @Component({
// //   selector: 'app-public-site-renderer',
// //   standalone: true,
// //   imports: [CommonModule, ModernTemplateComponent],
// //   templateUrl: './public-site-renderer.component.html',
// // })
// // export class PublicSiteRendererComponent implements OnInit {
// //   private route = inject(ActivatedRoute)
// //   private http = inject(HttpClient)

// //   site: any = null
// //   loading = false
// //   errorMessage = ''

// //   ngOnInit(): void {
// //     this.route.paramMap.subscribe((params) => {
// //       const slug = params.get('slug')
// //       const pageSlug = params.get('pageSlug')

// //       if (!slug) {
// //         this.errorMessage = 'Invalid website URL.'
// //         return
// //       }

// //       this.loadSite(slug, pageSlug)
// //     })
// //   }

// //   loadSite(slug: string, pageSlug: string | null) {
// //     this.loading = true
// //     this.errorMessage = ''

// //     this.http.get<any>(`${environment.apiUrl}/public/sites/by-slug/${slug}`).subscribe({
// //       next: (res) => {
// //         this.site = {
// //           ...res,
// //           activePageSlug: pageSlug,
// //         }

// //         this.loading = false
// //       },
// //       error: () => {
// //         this.loading = false
// //         this.errorMessage = 'Website not found.'
// //       },
// //     })
// //   }
// // }
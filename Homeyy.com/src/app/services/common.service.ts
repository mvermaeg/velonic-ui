import { Injectable, PLATFORM_ID, Inject, Renderer2, RendererFactory2 } from '@angular/core';
import { Router } from '@angular/router';
import { Title, Meta } from '@angular/platform-browser';
import { ToastrService } from 'ngx-toastr';
import { isPlatformBrowser } from '@angular/common';
import { BehaviorSubject } from 'rxjs/internal/BehaviorSubject';
import * as moment from 'moment';
import Swal from 'sweetalert2';

declare let LazyLoad: any;

@Injectable({ providedIn: 'root' })
export class CommonService {
  public defaultImage = '../../../assets/img/notfound.svg'
  public tokenname = 'projectnametoken';
  sitepath = '';
  isBrowser: any;
  isSpinnerVisible: boolean = false;
  public renderer: Renderer2;
  categories = new BehaviorSubject<any>([]);

  constructor(
    public rendererFactory: RendererFactory2,
    private router: Router,
    public toastr: ToastrService,
    private meta: Meta,
    private title: Title,
    @Inject(PLATFORM_ID) platformId: string,
  ) {
    this.renderer = rendererFactory.createRenderer(null, null);
    this.isBrowser = isPlatformBrowser(platformId);
  }

  public GenerateTags(tags: any) {
    tags = {
      title: 'Homeyy',
      description: 'Homeyy',
      keywords: 'Homeyy',
      image: '',
      path: '',
      ...tags
    };
    this.title.setTitle(tags.title);
    this.meta.updateTag({ name: 'Description', content: tags.description });
    this.meta.updateTag({ name: 'Keywords', content: tags.keywords });
    this.meta.updateTag({ name: 'twitter:card', content: 'summary' });
    this.meta.updateTag({ name: 'twitter:site', content: '@projectname' });
    this.meta.updateTag({ name: 'twitter:title', content: tags.title });
    this.meta.updateTag({ name: 'twitter:description', content: tags.description });
    this.meta.updateTag({ name: 'twitter:image', content: tags.image });
    this.meta.updateTag({ property: 'og:type', content: 'product' });
    this.meta.updateTag({ property: 'og:site_name', content: 'projectname' });
    this.meta.updateTag({ property: 'og:title', content: tags.title });
    this.meta.updateTag({ property: 'og:description', content: tags.description });
    this.meta.updateTag({ property: 'og:image', content: tags.image });
    this.meta.updateTag({ property: 'og:url', content: this.sitepath + '/' + tags.path });
  }
  public lazyload() {
    if (this.isBrowser) {
      setTimeout(() => new LazyLoad({ elements_selector: ".lazy" }), 100);
    }
  }

  public get isLoggedin(): any {
    if (this.isBrowser) {
      const TKN = localStorage.getItem(this.tokenname)
      if (TKN && TKN !== null) {
        const ANO = JSON.parse(this.Decrypt(TKN, this.tokenname)).AccountNo;
        return (ANO > 0) ? true : false
      }
      return false;
    }
  }

  public ProductUrl(str: string) {
    return str.toLowerCase().replace(/[^A-Z0-9]/ig, "-").replace(/[-]+/g, '-').trim()
  }

  public LocalStorageSet(name: string, data: any) {
    if (this.isBrowser) {
      return localStorage.setItem(name, this.Encrypt(JSON.stringify(data), name));
    }
  }

  public LocalStorageGet(name: string) {
    if (this.isBrowser) {
      return JSON.parse(this.Decrypt(localStorage.getItem(name), name));
    }
  }

  public SessionStorageSet(name: string, data: any) {
    if (this.isBrowser) {
      return sessionStorage.setItem(name, this.Encrypt(JSON.stringify(data), name));
    }
  }

  public SessionStorageGet(name: string) {
    if (this.isBrowser) {
      const Data = sessionStorage.getItem(name);
      if (Data && Data !== null) {
        return JSON.parse(this.Decrypt(Data, name));
      }
    }
  }

  public ConvertDropDown(array: []) {
    return array.map((o: any, i = 1) => ({ id: i + 1, itemName: o }))
  }

  public RemoveNull(data: any) {
    return new Set([].concat(...data.map(Object.keys)))
      .forEach(key => data.filter((obj: any) => obj[key] === 'no' || obj[key] === 'undefined' || obj[key] === undefined || obj[key] === null || obj[key] === 'No').forEach((obj: any) => obj[key] = 'NA'));
  }

  public Encrypt(o: any, salt: any) {
    o = JSON.stringify(o).split('');
    for (let i = 0, l = o.length; i < l; i++) {
      if (o[i] === '{') {
        o[i] = '}';
      } else if (o[i] === '}') {
        o[i] = '{';
      }
    }
    return btoa(encodeURI(salt + o.join('')));
  }

  public Decrypt(o: any, salt: any) {
    o = decodeURI(atob(o));
    if (salt && o.indexOf(salt) !== 0) {
      throw new Error('object cannot be decrypted');
    }
    o = o.substring(salt.length).split('');
    for (let i = 0, l = o.length; i < l; i++) {
      if (o[i] === '{') {
        o[i] = '}';
      } else if (o[i] === '}') {
        o[i] = '{';
      }
    }
    return JSON.parse(o.join(''));
  }

  public GotoURL(url: string) {
    this.router.navigate([url]);
  }

  public GotoURLParam(url: string) {
    this.router.navigateByUrl(url);
  }

  public AlphabetsOnly(event: any) {
    const charCode = event.keyCode;
    if ((charCode > 64 && charCode < 91) || (charCode > 96 && charCode < 123) || charCode === 8 || charCode === 32) {
      event.target.value = event.target.value.replace(/^[A-Za-z0-9-,.;'&@/.() ]*$/, '');
      return true;
    } else {
      return false;
    }
  }

  public numberOnly(event: any): boolean {
    const charCode = (event.which) ? event.which : event.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57)) {
      return false;
    }
    return true;
  }


  public Percentage(number: any, target: any) {
    return number / target * 100
  }

  public NoSpace(event: any) {
    if (event.keyCode !== 32) {
      // return false;
    }
    return false;
  }

  public RemoveDash(str: any) {
    return str.replace(/-/g, ' ').toLowerCase()
  }

  public UpperCase($event: any) {
    return $event.target.value.toUpperCase();
  }

  public LowerCase($event: any) {
    return $event.target.value.toLowerCase();
  }

  public GoTo($element: any): void {
    $element.scrollIntoView({ behavior: 'smooth', block: 'start', inline: 'nearest' });
  }

  public SwalSuccess(msg: string, heading = 'Success!') {
    Swal.fire({
      title: heading,
      text: msg,
      icon: 'success',
      timer: 5000 
    });
  }

}

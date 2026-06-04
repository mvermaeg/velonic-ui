export type MenuItem = {
  key?: string
  label?: string
  icon?: string
  link?: string
  collapsed?: boolean
  subMenu?: MenuItem[]
  isTitle?: boolean
  badge?: any
  parentKey?: string
  disabled?: boolean
}

export const MENU: MenuItem[] = [

   {
    key: 'dashboard',
    icon: 'ri-settings-3-line',
    label: 'Dashboard',
    link: '/dashboard',
  },
// *\------------- Leads ---------------\*
  {
    key: 'leads',
    icon: 'ri-group-line',
    label: 'Leads',
    link: '/leads/dashboard',
    collapsed: true,
    subMenu: [
       {
        key: 'leads-export',
        label: 'Export',
        link: '/leads/export',
        parentKey: 'leads',
      },
      {
        key: 'leads-sales',
        label: 'Sales',
        link: '/leads/sales',
        parentKey: 'leads'
      },
      {
        key: 'leads-leadform',
        label: 'Leadform',
        link: '/leads/leadform',
        parentKey: 'leads'
      },
      {
        key: 'leads-subids',
        label: 'Sub IDs',
        link: '/leads/subids',
        parentKey: 'leads'
      },
      {
        key: 'leads-returns',
        label: 'Returns',
        link: '/leads/returns',
        parentKey: 'leads'
      },
      {
        key: 'leads-rejections',
        label: 'Rejections',
        link: '/leads/rejections',
        parentKey: 'leads'
      },
      {
        key: 'leads-appointment-sales',
        label: 'Appointment & Sales',
        link: '/leads/appointment-sales',
        parentKey: 'leads'
      },
      {
        key: 'leads-appointment-sales',
        label: 'Alead test',
        link: '/leads/test',
        parentKey: 'leads'
      },
      {
  key: 'sites-media',
  label: 'Media',
  link: '/sites/media',
},
    ]
  },

  // *\------------- Sites ---------------\*
  {
    key: 'sites',
    icon: 'ri-map-pin-line',
    label: 'Sites',
    link: '/sites/pages',
    collapsed: true,
    subMenu: [
      {
        key: 'sites-pages',
        label: 'Pages',
        link: '/sites/pages',
        parentKey: 'sites'
      },
      {
  key: 'sites-domains',
  label: 'Domains',
  link: '/sites/domains',
},
      {
        key: 'sites-forms',
        label: 'Forms',
        link: '/sites/forms',
        parentKey: 'sites'
      },
      {
        key: 'sites-themes',
        label: 'Themes',
        link: '/sites/themes',
        parentKey: 'sites'
      },
      {
        key: 'sites-templates',
        label: 'Templates',
        link: '/sites/templates',
        parentKey: 'sites'
      },
      {
        key: 'sites-domains',
        label: 'Domains',
        link: '/sites/domains',
        parentKey: 'sites'
      },
      {
        key: 'sites-pixels',
        label: 'Pixels',
        link: '/sites/pixels',
        parentKey: 'sites'
      },
      {
  key: 'sites-forms',
  label: 'Forms',
  link: '/sites/forms',
},
    ]
  },

  // *\------------- Clients ---------------\*
  {
    key: 'clients',
    icon: 'ri-building-line',
    label: 'Clients',
    link: '/clients/crm',
    collapsed: true,
    subMenu: [
      
      {
        key: 'clients-crm',
        label: 'Dashboard',
        link: '/clients',
        parentKey: 'clients'
      },
      {
        key: 'clients-contractapi',
        label: 'Contract API',
        link: '/clients/contractapi',
        parentKey: 'clients'
      },
      {
        key: 'clients-vendorapi',
        label: 'Vendor API',
        link: '/clients/vendorapi',
        parentKey: 'clients'
      },
      {
        key: 'clients-deliveries',
        label: 'Deliveries',
        link: '/clients/deliveries',
        parentKey: 'clients'
      },
      {
        key: 'clients-bidding',
        label: 'Bidding',
        link: '/clients/bidding',
        parentKey: 'clients'
      },

       {
        key: 'bidding-settings',
        label: 'Bidding Settings',
        link: '/clients/bidding-settings',
        parentKey: 'clients'
      },
      {
        key: 'clients-contractissues',
        label: 'Contract Issues',
        link: '/clients/contractissues',
        parentKey: 'clients'
      }
    ]
  },

  // *\------------- Campaigns ---------------\*
  {
    key: 'campaigns',
    icon: 'ri-megaphone-line',
    label: 'Campaigns',
    link: '/dashboard',
  },

  // *\------------- Media Buying ---------------\*
  {
    key: 'media-buying',
    icon: 'ri-shopping-cart-line',
    label: 'Media Buying',
     link: '/media-buying/links',
    collapsed: true,
    subMenu: [
      {
        key: 'media-buying-links',
        label: 'Links',
        link: '/media-buying/links',
        parentKey: 'media-buying'
      },
      {
        key: 'media-buying-clicks',
        label: 'Clicks',
        link: '/media-buying/clicks',
        parentKey: 'media-buying'
      },
      {
        key: 'media-buying-split-test',
        label: 'Split Test',
        link: '/media-buying/split-test',
        parentKey: 'media-buying'
      },
      {
        key: 'media-buying-facebook-ads-review',
        label: 'Facebook Ads Review',
        link: '/media-buying/facebook-ads-review',
        parentKey: 'media-buying'
      },
      {
        key: 'media-buying-external',
        label: 'External',
        link: '/media-buying/external',
        parentKey: 'media-buying'
      },
      {
        key: 'media-buying-zip-repository',
        label: 'Zip Repository',
        link: '/media-buying/zip-repository',
        parentKey: 'media-buying'
      },
      {
        key: 'mediabuying-lead-inspector',
        label: 'Lead Inspector',
        link: '/mediabuying/lead-inspector',
        parentKey: 'mediabuying'
      },
      {
        key: 'mediabuying-ad-creatives',
        label: 'Ad Creatives',
        link: '/mediabuying/ad-creatives',
        parentKey: 'mediabuying'
      },
      {
        key: 'mediabuying-fb-ad-accounts',
        label: 'FB Ad Accounts',
        link: '/mediabuying/fb-ad-accounts',
        parentKey: 'mediabuying'
      }
    ]
  },

  // *\------------- Affiliates ---------------\*
  {
    key: 'affiliates',
    icon: 'ri-user-add-line',
    label: 'Affiliates',
     link: '/affiliates/frauds',
    collapsed: true,
    subMenu: [
      {
        key: 'affiliates-frauds',
        label: 'Frauds',
        link: '/affiliates/frauds',
        parentKey: 'affiliates'
      },
      {
        key: 'affiliates-clicks',
        label: 'Clicks',
        link: '/affiliates/clicks',
        parentKey: 'affiliates'
      },
      {
        key: 'affiliates-commissions',
        label: 'Commissions',
        link: '/affiliates/commissions',
        parentKey: 'affiliates'
      },
      {
        key: 'affiliates-offers',
        label: 'Offers',
        link: '/affiliates/offers',
        parentKey: 'affiliates'
      },
      {
        key: 'affiliates-pixel-log',
        label: 'Pixel Log',
        link: '/affiliates/pixel-log',
        parentKey: 'affiliates'
      },
      {
        key: 'affiliates-email-subscriptions',
        label: 'Email Subscriptions',
        link: '/affiliates/email-subscriptions',
        parentKey: 'affiliates'
      }
    ]
  },

  // *\------------- Vendors ---------------\*
  {
    key: 'vendors',
    icon: 'ri-box-3-line',
    label: 'Vendors',
    link: '/vendors/leads-io',
    collapsed: true,
    subMenu: [
      {
        key: 'vendors-leads-io',
        label: 'Leads I/O',
        link: '/vendors/leads-io',
        parentKey: 'vendors'
      },
      {
        key: 'vendors-api-keys',
        label: 'API Keys',
        link: '/vendors/api-keys',
        parentKey: 'vendors'
      },
      {
        key: 'vendors-commissions',
        label: 'Commissions',
        link: '/vendors/commissions',
        parentKey: 'vendors'
      },
      {
        key: 'vendors-blacklist',
        label: 'Blacklist',
        link: '/vendors/blacklist',
        parentKey: 'vendors'
      },
      {
        key: 'vendors-lead-quality',
        label: 'Lead Quality',
        link: '/vendors/lead-quality',
        parentKey: 'vendors'
      },
      {
        key: 'vendors-ringba-api-specs',
        label: 'Ringba API Specs',
        link: '/vendors/ringba-api-specs',
        parentKey: 'vendors'
      },
      {
        key: 'vendors-retreaver-api-specs',
        label: 'Retreaver API Specs',
        link: '/vendors/retreaver-api-specs',
        parentKey: 'vendors'
      }
    ]
  },

  // *\------------- Reports ---------------\*
  {
    key: 'reports',
    icon: 'ri-line-chart-line',
    label: 'Reports',
    link: '/reports/general',
    collapsed: true,
    subMenu: [
      {
        key: 'reports-general',
        label: 'General',
        link: '/reports/general',
        parentKey: 'reports'
      },
      {
        key: 'reports-campaign',
        label: 'Campaign',
        link: '/reports/campaign',
        parentKey: 'reports'
      },
      {
        key: 'reports-contracts',
        label: 'Contracts',
        link: '/reports/contracts',
        parentKey: 'reports'
      },
      {
        key: 'reports-vendors',
        label: 'Vendors',
        collapsed: true,
        parentKey: 'reports',
         link: '/reports/vendors/daily',
        subMenu: [
          {
            key: 'reports-vendors-daily',
            label: 'Daily',
            parentKey: 'reports-vendors',
            link: '/reports/vendors/daily',
          },
          {
            key: 'reports-vendors-subid',
            label: 'Sub ID',
            parentKey: 'reports-vendors',
            link: '/reports/vendors/sub-id',
          },
        ],
      },
      {
        key: 'reports-affiliates',
        label: 'Affiliates',
        collapsed: true,
        parentKey: 'reports',
        subMenu: [
          {
            key: 'reports-affiliates-daily-report',
            label: 'Daily-Report',
            parentKey: 'reports-affiliates',
            link: '/reports/affiliates/daily-report',
          },
          {
            key: 'reports-affiliates-conversions',
            label: 'Conversions',
            parentKey: 'reports-affiliates',
            link: '/reports/affiliates/conversions',
          },
        ],
      },
    ]
  },



  // *\------------- System ---------------\*
  {
    key: 'system',
    icon: 'ri-server-line',
    label: 'System',
    link: '/dashboard',
  },

  // *\------------- Setting ---------------\*
  {
    key: 'setting',
    icon: 'ri-settings-3-line',
    label: 'Setting',
    link: '/dashboard',
  },

]

export const HORIZONTAL_MENU_ITEMS: MenuItem[] = [

]

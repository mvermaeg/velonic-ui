import { currency } from '@common/constants'
import type { ChartOptions } from '@common/apexchart.model'

type Statistic = {
  title: string
  stats: string
  change: string
  icon: string
  variant: string
}

type ProjectData = {
  id: number
  projectName: string
  startDate: string
  dueDate: string
  status: string
  variant: string
}

type Leads = {
  id: string
  email: string
  name: string
  campagin: string
  time: string
  revenue: string
}


type Campaign = {
  campagin: string,
  revenue: string,
  cost: string
}

type Clients = {
  clients: string
  revenue: string,
}

type Contracts = {
  contracts: string
  revenue: string,
}

export const statistics: Statistic[] = [
  {
    title: 'Daily Visits',
    stats: '8,652',
    change: '2.97%',
    icon: 'ri-eye-line',
    variant: 'text-bg-pink',
  },
  {
    title: 'Revenue',
    stats: currency + '9,254.62',
    change: '18.25%',
    icon: 'ri-wallet-2-line',
    variant: 'text-bg-purple',
  },
  {
    title: 'Orders',
    stats: '753',
    change: '-5.75%',
    icon: 'ri-shopping-basket-line',
    variant: 'text-bg-info',
  },
  {
    title: 'Users',
    stats: '63,154',
    change: '8.21%',
    icon: 'ri-group-2-line',
    variant: 'text-bg-primary',
  },
]

export const chatMessages = [
  {
    id: 1,
    userPic: 'assets/images/users/avatar-5.jpg',
    userName: 'Geneva',
    text: 'Hello!',
    postedOn: '10:00',
  },
  {
    id: 2,
    userPic: 'assets/images/users/avatar-1.jpg',
    userName: 'Thomson',
    text: 'Hi, How are you? What about our next meeting?',
    postedOn: '10:01',
  },
  {
    id: 3,
    userPic: 'assets/images/users/avatar-5.jpg',
    userName: 'Geneva',
    text: 'Yeah everything is fine',
    postedOn: '10:01',
  },
  {
    id: 4,
    userPic: 'assets/images/users/avatar-1.jpg',
    userName: 'Thomson',
    text: "Wow that's great",
    postedOn: '10:02',
  },
  {
    id: 5,
    userPic: 'assets/images/users/avatar-5.jpg',
    userName: 'Geneva',
    text: 'Cool!',
    postedOn: '10:03',
  },
]


export const Leads: Leads[] = [
  {
    id: '369-268-371',
    email: 'vambriz64@gmail.com',
    name: 'Veronica Ambriz',
    campagin: 'H-RF - Roofing',
    time: '04:10',
    revenue: '$4.05',
  },
  {
    id: '369-268-372',
    email: 'gmancha43@gmail.com',
    name: 'George Mancha',
    campagin: 'H-RF - Roofing',
    time: '04:09',
    revenue: '$0.00',
  },
  {
    id: '369-268-373',
    email: 'lpugh81@gmail.com',
    name: 'Lorenzo Pugh',
    campagin: 'H-RF - Roofing',
    time: '04:09',
    revenue: '$0.00',
  },
  {
    id: '369-268-374',
    email: 'butterbearcup42@yahoo.com',
    name: 'Mary Owens',
    campagin: 'H-RF - Roofing',
    time: '04:04',
    revenue: '$4.05',
  },
  {
    id: '369-268-375',
    email: 'arthurgarley@gmail.com',
    name: 'Arthur Garley',
    campagin: 'H-RF - Roofing',
    time: '04:02',
    revenue: '$60.00',
  },
  {
    id: '369-268-376',
    email: 'daniel.r.stec@gm.com',
    name: 'Dan Stec',
    campagin: 'H-SD - Siding',
    time: '04:01',
    revenue: '$0.00',
  },
  {
    id: '369-268-377',
    email: 'maidashiflett@gmail.com',
    name: 'Maida Shiflett',
    campagin: 'H-HV - HVAC',
    time: '04:00',
    revenue: '$0.00',
  },
  {
    id: '369-268-378',
    email: 'toddnloribroussard@gmail.com',
    name: 'Todd Broussard',
    campagin: 'H-RF - Roofing',
    time: '03:59',
    revenue: '$0.00',
  },
  {
    id: '369-268-379',
    email: 'maryresa66@gmail.com',
    name: 'Mary Bailey',
    campagin: 'H-SD - Siding',
    time: '03:57',
    revenue: '$0.00',
  },
  {
    id: '369-268-380',
    email: 'whizkello@yahoo.com',
    name: 'Harry Kelloway',
    campagin: 'H-RF - Roofing',
    time: '03:56',
    revenue: '$0.00',
  }
]

export const Campagin: Campaign[] = [
  {
    campagin: 'H-RF - Roofing',
    revenue: '$1,973',
    cost: '$1,449'
  },
  {
    campagin: 'H-SD - Siding',
    revenue: '$822',
    cost: '$581'
  },
  {
    campagin: 'H-HV - HVAC',
    revenue: '$763',
    cost: '$808'
  },
  {
    campagin: 'H-BR - Bathroom',
    revenue: '$690',
    cost: '$609'
  },
  {
    campagin: 'H-WI - Windows',
    revenue: '$104',
    cost: '$48'
  },
  {
    campagin: 'H-WT - Walk-in Tub',
    revenue: '$36',
    cost: '$19'
  },
  {
    campagin: 'L-MV - MVA',
    revenue: '$0',
    cost: '$13'
  }, {
    campagin: 'H-RF - Roofing',
    revenue: '$1,973',
    cost: '$1,449'
  },
  {
    campagin: 'H-SD - Siding',
    revenue: '$822',
    cost: '$581'
  },
  {
    campagin: 'H-HV - HVAC',
    revenue: '$763',
    cost: '$808'
  },
]

export const Clients: Clients[] = [
  {
    clients: 'Networx',
    revenue: '$1,327'
  },
  {
    clients: 'DaBella',
    revenue: '$700'
  },
  {
    clients: 'QuinStreet',
    revenue: '$603'
  },
  {
    clients: 'PX',
    revenue: '$418'
  },
  {
    clients: 'HomeBuddy',
    revenue: '$402'
  },
  {
    clients: 'Angi',
    revenue: '$248'
  },
  {
    clients: 'Home Appointments',
    revenue: '$198'
  },
  {
    clients: 'Blue Ink Digital',
    revenue: '$145'
  },
  {
    clients: 'Contractor Appointments',
    revenue: '$140'
  },
  {
    clients: 'Canopy Home Exteriors',
    revenue: '$81'
  }
]

export const Contracts: Contracts[] = [
  {
    contracts: 'H-RF-73 - Roofing - Networx',
    revenue: '$535'
  },
  {
    contracts: 'H-HV-922 - HVAC - PX',
    revenue: '$291'
  },
  {
    contracts: 'H-RF-1283 - Roofing - HomeBuddy',
    revenue: '$260'
  },
  {
    contracts: 'H-SD-506 - Siding - Networx',
    revenue: '$246'
  },
  {
    contracts: 'H-RF-1482 - Roofing - QuinStreet',
    revenue: '$177'
  },
  {
    contracts: 'H-HV-109 - HVAC - Networx',
    revenue: '$171'
  },
  {
    contracts: 'H-RF-1962 - Roofing - Angi',
    revenue: '$171'
  },
  {
    contracts: 'H-SD-1488 - Siding - QuinStreet ',
    revenue: '$164'
  },
  {
    contracts: 'H-BR-1484 - Bathroom - QuinStreet',
    revenue: '$154'
  },
  {
    contracts: 'H-BR-1472 - Blue Ink - FCC ',
    revenue: '$145'
  }
]

export const lineChartOpts: Partial<ChartOptions> = {
  chart: {
    height: 380,
    type: 'line',
    zoom: {
      enabled: false,
    },
  },
  dataLabels: {
    enabled: false,
  },
  colors: ['#487FFF','#45B369','#F4941E','#8252E9','#DE3ACE','#00B8F2'],
  stroke: {
    width: [3,3,3,3,3,3],
    curve: 'straight',
  },
  series: [
   {
      name: 'Revenue',
      data: [1420, 2560, 1745, 2330, 1355, 2890, 1985, 3120, 2215, 2675, 3540, 2785],
    },
    {
      name: 'Clicks',
      data: [1350, 2450, 3650, 1240, 2030, 3150, 2240, 2870, 1980, 2650, 3090, 2760],
    },
    {
      name: 'Leads',
      data: [1120, 1230, 1320, 1440, 1530, 1650, 1740, 1870, 1680, 1750, 1890, 1760],
    },
    {
      name: 'ROI',
      data: [620, 930, 220, 840, 130, 1050, 400, 1080, 650, 150, 1090, 960],
    },
    {
      name: 'Completion Rate',
      data: [620, 730, 820, 940, 1030, 1150, 1040, 1180, 1080, 1250, 1190, 1060],
    },
    {
      name: 'Conversion Rate',
      data: [420, 530, 1620, 740, 830, 1950, 840, 980, 1880, 950, 990, 1860],
    }
  ],
  title: {
    text: '',
    align: 'center',
  },
  grid: {
    row: {
      colors: ['transparent', 'transparent'], // takes an array which will be repeated on columns
      opacity: 0.2,
    },
    borderColor: '#e1e1e2',
  },
  // labels: series.monthDataSeries1.dates,
  xaxis: {
    categories: ['20:00', '22;00', '00:00', '02:00', '04:00', '06:00', '08:00', '10:00', '12:00', '14:00', '16:00', '18:00'],
  },
  responsive: [
    {
      breakpoint: 600,
      options: {
        chart: {
          toolbar: {
            show: false,
          },
        },
        legend: {
          show: false,
        },
      },
    },
  ],
}
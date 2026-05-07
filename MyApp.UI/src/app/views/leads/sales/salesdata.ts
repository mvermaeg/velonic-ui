export type DataTableItemsType = {
  id: number
  customer: string
  service: string
  amount: string
  date: string
  status: string
}

export const DataTableItems: DataTableItemsType[] = [
  {
    id: 1,
    customer: 'John Smith',
    service: 'AC Installation',
    amount: '4,500',
    date: '2026-03-14',
    status: 'Closed'
  },
  {
    id: 2,
    customer: 'Mary Johnson',
    service: 'Heating System',
    amount: '3,200',
    date: '2026-03-14',
    status: 'Pending'
  },
  {
    id: 3,
    customer: 'Robert Davis',
    service: 'HVAC Maintenance',
    amount: '850',
    date: '2026-03-12',
    status: 'Closed'
  },{
    id: 4,
    customer: 'Lisa Wilson',
    service: 'Duct Cleaning',
    amount: '650',
    date: '2026-03-11',
    status: 'Closed'
  }
]

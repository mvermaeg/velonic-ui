export type DataTableItemsType = {
  id: number
  name: string
  offerlink: string
  campaignname: string
  sourcename: string
  created: string
  updated:string
}

export const DataTableItems: DataTableItemsType[] = [
  {
    id: 1,
    name: "Affiliate Network A - HVAC Offer",
    offerlink: "https://affnetwork.com/offer/hvac-123",
    campaignname: "Summer AC Special",
    sourcename: "Affiliate Network A",
    created: "2025-08-15",
    updated: "2026-03-10"
  },
  {
    id: 2,
    name: "Partner B - Roofing Leads",
    offerlink: "https://partnerb.net/roofing-leads",
    campaignname: "Roofing Services",
    sourcename: "Partner Network B",
    created: "2025-09-20",
    updated: "2026-03-12"
  },
  {
    id: 3,
    name: "Solar Lead Exchange",
    offerlink: "https://solarleads.com/exchange/solar-456",
    campaignname: "Solar Panel Install",
    sourcename: "Solar Lead Exchange",
    created: "2025-10-05",
    updated: "2026-03-14"
  }
]
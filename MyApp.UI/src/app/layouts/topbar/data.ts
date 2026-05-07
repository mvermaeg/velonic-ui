function subtractHours(date: Date, minutes: number) {
  date.setMinutes(date.getMinutes() - minutes)
  return date
}

export interface NotificationItem {
  id: number
  title: string
  icon: string
  variant: string
  createdAt: Date
}

export interface ProfileOption {
  label: string
  icon: string
  redirectTo: string
}

/**
 * notification items
 */
export const Notifications: NotificationItem[] = [
  {
    id: 1,
    title: 'Caleb Flakelar commented on Admin',
    icon: 'mdi mdi-comment-account-outline',
    variant: 'primary',
    createdAt: subtractHours(new Date(), 1),
  },
  {
    id: 2,
    title: 'New user registered.',
    icon: 'mdi mdi-account-plus',
    variant: 'warning',
    createdAt: subtractHours(new Date(), 300),
  },
  {
    id: 3,
    title: 'Carlos Crouch liked',
    icon: 'mdi mdi-heart',
    variant: 'danger',
    createdAt: subtractHours(new Date(), 4320),
  },
  {
    id: 4,
    title: 'Caleb Flakelar commented on Admi',
    icon: 'mdi mdi-comment-account-outline',
    variant: 'pink',
    createdAt: subtractHours(new Date(), 5760),
  },
  {
    id: 5,
    title: 'New user registered.',
    icon: 'mdi mdi-account-plus',
    variant: 'purple',
    createdAt: subtractHours(new Date(), 10960),
  },
  {
    id: 6,
    title: 'Carlos Crouch liked Admin',
    icon: 'mdi mdi-heart',
    variant: 'success',
    createdAt: subtractHours(new Date(), 10960),
  },
]

export const profileMenus: ProfileOption[] = [
  {
    label: 'My Account',
    icon: 'ri-account-circle-line',
    redirectTo: '/pages/profile',
  },
  {
    label: 'Support',
    icon: 'ri-customer-service-2-line',
    redirectTo: '/pages/faq',
  },
    {
    label: 'Settings',
    icon: 'ri-settings-4-line',
    redirectTo: '/pages/profile',
  },
  {
    label: 'Logout',
    icon: 'ri-logout-box-line',
    redirectTo: '/auth/logout',
  },
]

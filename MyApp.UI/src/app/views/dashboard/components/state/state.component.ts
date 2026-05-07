import { CommonModule } from '@angular/common';
import { Component } from '@angular/core'
import { statistics } from '@views/dashboard/data'

@Component({
  selector: 'dashboard-state',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './state.component.html',
  styleUrl: './state.component.scss'
})
export class StateComponent {
  statistics = statistics

  stats = [
    {
      title: 'Revenue Today',
      value: '$35,807',
      change: '+5000',
      note: 'Last 30 days revenue',
      icon: 'ri-money-dollar-circle-line',
      color: 'blue',
      trend: 'up'
    },
    {
      title: 'MB Profit Today',
      value: '$8,307',
      change: '+200',
      note: 'Last 30 days users',
      icon: 'ri-money-dollar-circle-line',
      color: 'yellow',
      trend: 'up'
    },
    {
      title: 'Profit Today',
      value: '$5,807',
      change: '-800',
      note: 'Last 30 days subscription',
      icon: 'ri-money-dollar-circle-line',
      color: 'green1',
      trend: 'down'
    },

    {
      title: 'AF Profit Today',
      value: '$732',
      change: '+200',
      note: 'Last 30 days users',
      icon: 'ri-money-dollar-circle-line',
      color: 'purple',
      trend: 'up'
    },

    {
      title: 'Cost Today',
      value: '$28,690',
      change: '+$5,000',
      note: 'Last 30 days expense',
      icon: 'ri-money-dollar-circle-line',
      color: 'cyan',
      trend: 'up'
    },
    {
      title: 'VN Profit Today',
      value: '$0',
      change: '+200',
      note: 'Last 30 days users',
      icon: 'ri-money-dollar-circle-line',
      color: 'pink',
      trend: 'up'
    },
  ];

}


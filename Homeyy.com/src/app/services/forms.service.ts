import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class FormsService {

    qoutes = [
      {
        title: 'AC & Heat',
        path: 'hvac',
        icon: '../../../../assets/images/services/hvac.jpg'
      },
      {
        title: 'Bathroom',
        path: 'bathroom',
        icon: '../../../../assets/images/services/bathroom.jpg'
      },
      {
        title: 'Kitchen',
        path: 'kitchen',
        icon: '../../../../assets/images/services/kitchen.jpg'
      },
      {
        title: 'Plumbing',
        path: 'plumbing',
        icon: '../../../../assets/images/services/plumbing.jpg'
      },
      {
        title: 'Window',
        path: 'window',
        icon: '../../../../assets/images/services/windows.webp'
      },
      {
        title: 'Door',
        path: 'door',
        icon: '../../../../assets/images/services/door.jpg'
      },
      {
        title: 'Flooring',
        path: 'flooring',
        icon: '../../../../assets/images/services/flooring.jpg'
      },
      {
        title: 'Gutter',
        path: 'gutter',
        icon: '../../../../assets/images/services/gutter.jpg'
      },
      {
        title: 'Fencing',
        path: 'fencing',
        icon: '../../../../assets/images/services/fencing.jpg'
      },
      {
        title: 'Solar',
        path: 'solar',
        icon: '../../../../assets/images/services/solar.jpg'
      },
      {
        title: 'Roofing',
        path: 'roofing',
        icon: '../../../../assets/images/services/roofing.webp'
      },
      {
        title: 'Home Security',
        path: 'home-security',
        icon: '../../../../assets/images/services/security.jpg'
      },
      {
        title: 'Siding',
        path: 'siding',
        icon: '../../../../assets/images/services/siding.jpg'
      }
    ];

    roofing:any= [
        {
          title: "What's the nature of your project?",
          type: 'buttons',
          field: 'projectType',
          options: [
            'Replace Roof',
            'Repair Roof',
            'Install New Roof'
          ]
        },
        {
          title: 'What type of roofing do you need?',
          type: 'buttons',
          field: 'roofType',
          options: [
            'Asphalt Shingles',
            'Metal Roofing',
            'Tile Roofing',
            'cedar Shake',
            'Tar Roofing',
            'Natural Slate'
          ]
        },
        {
          title: 'Do you own your home?',
          type: 'buttons',
          field: 'homeOwner',
          options: ['Yes', 'No']
        }
      ];

      bathroom:any= [
        {
          title: 'What type of bathroom project?',
          type: 'buttons',
          field: 'projectType',
          options: [
            'Complete Bathroom Remodel',
            'Bathwraps',
            'Cabinets vanity',
            'Countrtops',
            'Flooring',
            'Bath to shower',
            'Sinks and Faucets',
            'Toilets',
            'walk-in-shower',
            'Others'
          ]
        },
        {
          title: 'How many bathrooms?',
          type: 'buttons',
          field: 'bathrooms',
          options: [
            '1',
            '2',
            '3+'
          ]
        },
        {
          title: 'Do you own your home?',
          type: 'buttons',
          field: 'homeOwner',
          options: ['Yes', 'No']
        }
      ];

      window:any= [
        {
          title: 'What type of window project?',
          type: 'buttons',
          field: 'projectType',
          options: [
            'Replace window',
            'Repair window',
            'Install New window'
          ]
        },
        {
          title: 'How many windows does your project involve?',
          type: 'buttons',
          field: 'windowCount',
          options: [
            '1',
            '2',
            '3-5',
            '6-9',
            '10+'
          ]
        },
        {
          title: 'Do you own your home?',
          type: 'buttons',
          field: 'homeOwner',
          options: ['Yes', 'No']
        }
      ];

      hvac:any= [
        {
          title: "What's the nature of your project?",
          type: 'buttons',
          field: 'projectType',
          options: [
            'Replace Service',
            'Repair Service',
            'Install New System'
          ]
        },
        {
          title: 'What type of air system are you interested in?',
          type: 'buttons',
          field: 'airType',
          options: [
            'Cooling',
            'Heating',
          ]
        },
        {
          title: 'What type of system are you interested in?',
          type: 'buttons',
          field: 'systemType',
          options: ['Central A/C', 'ductless mini-split', 'Ducts/Vents', 'Gas Boiler', 'Propane Boiler', 'Oil Boiler', 'Electric Bolier', 'Gas Furnace']
        },
        {
          title: 'Do you own your home?',
          type: 'buttons',
          field: 'homeOwner',
          options: ['Yes', 'No']
        }
      ];

      siding:any= [
        {
          title: "What's the nature of your project?",
          type: 'buttons',
          field: 'projectType',
          options: [
            'Replace siding',
            'Repair siding',
            'Install New siding'
          ]
        },
        {
          title: 'What type of siding material do you need?',
          type: 'buttons',
          field: 'windowCount',
          options: [
            'Stucco',
            'Metal',
            'Alluminum',
            'Brick or Stone',
            'Fiber Cement Board',
            'Vinyl',
            'Wood'
          ]
        },
        {
          title: 'Do you own your home?',
          type: 'buttons',
          field: 'homeOwner',
          options: ['Yes', 'No']
        }
      ];

       solar:any= [
        {
          title: 'What do you need help with?',
          type: 'buttons',
          field: 'projectType',
          options: [
            'Solar Install',
            'Solar Replace',
            'Solar Repair'
          ]
        },
        {
          title: 'how much sunlight does your roof get?',
          type: 'buttons',
          field: 'roofType',
          options: [
           'A lot of sun',
           'Some sun',
           'not much sun',
           'uncertain'
          ]
        },
        {
          title: 'Do you own your home?',
          type: 'buttons',
          field: 'homeOwner',
          options: [
            'Yes',
            'No'
          ]
        }
      ];

      kitchen:any= [
        {
          title: 'What do you need help with?',
          type: 'buttons',
          field: 'systemType',
          options: [
            'Appliance',
            'cabinets install',
            "Cabinet Repair",
            'Countertops',
            'Flooring',
            'Floorplan',
            'Full kitchen',
            'Other'
          ]
        },
        {
          title: 'Do you own your home?',
          type: 'buttons',
          field: 'homeOwner',
          options: [
            'Yes',
            'No'
          ]
        }
      ];

      plumbing:any= [
        {
          title: 'What do you need help with?',
          type: 'buttons',
          field: 'projectType',
          options: [
           'Full residential repipe','Pex pipe','Copper repipe',
           'Bathtubs','commercial plumbing','Drain cleaning','Faucets','Gas pipes',
           'General repair','Leak repair','Sewer line repair',
           'shower','sump pump','Toilets','Water heaters','Other'

          ]
        },
       
        {
          title: 'Do you own your home?',
          type: 'buttons',
          field: 'homeOwner',
          options: [
            'Yes',
            'No'
          ]
        }
      ];

      door:any= [
        {
          title: 'what best describes your project?',
          type: 'buttons',
          field: 'projectType',
          options: [
            'Replace Door',
            'Repair Door',
            'Install New Door'
          ]
        },
        {
          title: 'What type of door project is this?',
          type: 'buttons',
          field: 'doorCount',
          options: [
           'exterior door',
           'interior door',
           'garage door',
           'sliding door',
           'storm door',
           'security door',
           'glass shower door',
          ]
        },
        {
          title: 'Do you own your home?',
          type: 'buttons',
          field: 'homeOwner',
          options: [
            'Yes',
            'No'
          ]
        }
      ];

      flooring:any= [
        {
          title: 'what do you need help with?',
          type: 'buttons',
          field: 'projectType',
          options: [
            'Replace Flooring',
            'Repair Flooring',
            'Install New Flooring'
          ]
        },
        {
          title: 'What type of flooring material do you need?',
          type: 'buttons',
          field: 'flooringType',
          options: [
            'Vinyl',
            'Woodern',
            'carpet',
            'Tile',
            'Laminate',
            'Concrete',
            'stone',
            'composite',
            'Other'
          ]
        },
         {
          title: 'Do you own your home?',
          type: 'buttons',
          field: 'homeOwner',
          options: [
            'Yes',
            'No'
          ]
        }
      ];
        
      gutter:any= [
        {
          title: 'what do you need help with?',
          type: 'buttons',
          field: 'projectType',
          options: [
            'Replace gutter',
            'Repair gutter',
            'Install New gutter',
            'Close'
          ]
        },
        {
          title: 'What type of gutter material do you need?',
          type: 'buttons',
          field: 'gutterType',
          options: [
            'copper',
            'Galvanized steel',
            'PVC',
            'Wood','seamless metal','Other'
          ]
        },
         {
          title: 'Do you own your home?',
          type: 'buttons',
          field: 'homeOwner',
          options: [
            'Yes',
            'No'
          ]
        }
      ];

      fencing:any= [
        {
          title: 'what is the nature of your project?',
          type: 'buttons',
          field: 'projectType',
          options: [
            'Replace Fencing',
            'Repair Fencing',
            'Install New Fencing'
          ]
        },
        {
          title: 'What type of fencing material do you need?',
          type: 'buttons',
          field: 'fencingType',
          options: [
            'Wood',
            'pvc',
            'Vinyl',
            'Chain link',
            'Wrought iron',
            'Alluminum',
            'Barbed wire',
            'Electric pet fence',
            'Other'
          ]
        },
         {
          title: 'Do you own your home?',
          type: 'buttons',
          field: 'homeOwner',
          options: [
            'Yes',
            'No'
          ]
        }
      ];

      homesecurity:any= [
        {
          title: 'what do you need help with?',
          type: 'buttons',
          field: 'projectType',
          options: [
            'Maintenance and Repair',
            'Install New home security system',
          ]
        },
        {
          title: 'What type of home security system do you need?',
          type: 'buttons',
          field: 'securityType',
          options: [
            'Security Cameras',
            'fire alarm system',
            'panic buttons',
            'Week leak detector',
          ]
        },
         {
          title: 'Do you own your home?',
          type: 'buttons',
          field: 'homeOwner',
          options: [
            'Yes',
            'No'
          ]
        }
      ]



}


// import { Injectable } from '@angular/core';

// @Injectable({
//     providedIn: 'root'
// })
// export class FormsService {

//     qoutes = [
//       {
//         title: 'AC & Heat',
//         path: 'hvac',
//         icon: '../../../../assets/images/services/hvac.jpg'
//       },
//       {
//         title: 'Bathroom',
//         path: 'bathroom',
//         icon: '../../../../assets/images/services/bathroom.jpg'
//       },
//       {
//         title: 'Kitchen',
//         path: 'kitchen',
//         icon: '../../../../assets/images/services/kitchen.jpg'
//       },
//       {
//         title: 'Plumbing',
//         path: 'plumbing',
//         icon: '../../../../assets/images/services/plumbing.jpg'
//       },
//       {
//         title: 'Window',
//         path: 'window',
//         icon: '../../../../assets/images/services/windows.webp'
//       },
//       {
//         title: 'Door',
//         path: 'door',
//         icon: '../../../../assets/images/services/door.jpg'
//       },
//       {
//         title: 'Flooring',
//         path: 'flooring',
//         icon: '../../../../assets/images/services/flooring.jpg'
//       },
//       {
//         title: 'Gutter',
//         path: 'gutter',
//         icon: '../../../../assets/images/services/gutter.jpg'
//       },
//       {
//         title: 'Fencing',
//         path: 'fencing',
//         icon: '../../../../assets/images/services/fencing.jpg'
//       },
//       {
//         title: 'Solar',
//         path: 'solar',
//         icon: '../../../../assets/images/services/solar.jpg'
//       },
//       {
//         title: 'Roofing',
//         path: 'roofing',
//         icon: '../../../../assets/images/services/roofing.webp'
//       },
//       {
//         title: 'Home Security',
//         path: 'home-security',
//         icon: '../../../../assets/images/services/security.jpg'
//       },
//       {
//         title: 'Siding',
//         path: 'siding',
//         icon: '../../../../assets/images/services/siding.jpg'
//       }
//     ];

//     roofing:any= [
//         {
//           title: "What's the nature of your project?",
//           type: 'buttons',
//           field: 'projectType',
//           options: [
//             'Replace Roof',
//             'Repair Roof',
//             'Install New Roof'
//           ]
//         },
//         {
//           title: 'What type of roofing do you need?',
//           type: 'buttons',
//           field: 'roofType',
//           options: [
//             'Asphalt Shingles',
//             'Metal Roofing',
//             'Tile Roofing',
//             'cedar Shake',
//             'Tar Roofing',
//             'Natural Slate'
//           ]
//         },
//         {
//           title: 'Do you own your home?',
//           type: 'buttons',
//           field: 'homeOwner',
//           options: ['Yes', 'No']
//         }
//       ];

//       bathroom:any= [
//         {
//           title: 'What type of bathroom project?',
//           type: 'buttons',
//           field: 'projectType',
//           options: [
//             'Complete Bathroom Remodel',
//             'Bathwraps',
//             'Cabinets vanity',
//             'Countrtops',
//             'Flooring',
//             'Bath to shower',
//             'Sinks and Faucets',
//             'Toilets',
//             'walk-in-shower',
//             'Others'
//           ]
//         },
//         {
//           title: 'How many bathrooms?',
//           type: 'buttons',
//           field: 'bathrooms',
//           options: [
//             '1',
//             '2',
//             '3+'
//           ]
//         },
//         {
//           title: 'Do you own your home?',
//           type: 'buttons',
//           field: 'homeOwner',
//           options: ['Yes', 'No']
//         }
//       ];

//       window:any= [
//         {
//           title: 'What type of window project?',
//           type: 'buttons',
//           field: 'projectType',
//           options: [
//             'Replace window',
//             'Repair window',
//             'Install New window'
//           ]
//         },
//         {
//           title: 'How many windows does your project involve?',
//           type: 'buttons',
//           field: 'windowCount',
//           options: [
//             '1',
//             '2',
//             '3-5',
//             '6-9',
//             '10+'
//           ]
//         },
//         {
//           title: 'Do you own your home?',
//           type: 'buttons',
//           field: 'homeOwner',
//           options: ['Yes', 'No']
//         }
//       ];

//       hvac:any= [
//         {
//           title: "What's the nature of your project?",
//           type: 'buttons',
//           field: 'projectType',
//           options: [
//             'Replace Service',
//             'Repair Service',
//             'Install New System'
//           ]
//         },
//         {
//           title: 'What type of air system are you interested in?',
//           type: 'buttons',
//           field: 'windowCount',
//           options: [
//             'Cooling',
//             'Heating',
//           ]
//         },
//         {
//           title: 'What type of system are you interested in?',
//           type: 'buttons',
//           field: 'homeOwner',
//           options: ['Central A/C', 'ductless mini-split', 'Ducts/Vents', 'Gas Boiler', 'Propane Boiler', 'Oil Boiler', 'Electric Bolier', 'Gas Furnace']
//         }
//       ];

//       siding:any= [
//         {
//           title: "What's the nature of your project?",
//           type: 'buttons',
//           field: 'projectType',
//           options: [
//             'Replace siding',
//             'Repair siding',
//             'Install New siding'
//           ]
//         },
//         {
//           title: 'What type of siding material do you need?',
//           type: 'buttons',
//           field: 'windowCount',
//           options: [
//             'Stucco',
//             'Metal',
//             'Alluminum',
//             'Brick or Stone',
//             'Fiber Cement Board',
//             'Vinyl',
//             'Wood'
//           ]
//         },
//         {
//           title: 'Do you own your home?',
//           type: 'buttons',
//           field: 'homeOwner',
//           options: ['Yes', 'No']
//         }
//       ];

//        solar:any= [
//         {
//           title: 'What do you need help with?',
//           type: 'buttons',
//           field: 'projectType',
//           options: [
//             'Solar Install',
//             'Solar Replace',
//             'Solar Repair'
//           ]
//         },
//         {
//           title: 'how much sunlight does your roof get?',
//           type: 'buttons',
//           field: 'roofType',
//           options: [
//            'A lot of sun',
//            'Some sun',
//            'not much sun',
//            'uncertain'
//           ]
//         },
//         {
//           title: 'Do you own your home?',
//           type: 'buttons',
//           field: 'homeOwner',
//           options: [
//             'Yes',
//             'No'
//           ]
//         }
//       ];

//       kitchen:any= [
//         {
//           title: 'What do you need help with?',
//           type: 'buttons',
//           field: 'systemType',
//           options: [
//             'Appliance',
//             'cabinets install',
//             "Cabinet Repair",
//             'Countertops',
//             'Flooring',
//             'Floorplan',
//             'Full kitchen',
//             'Other'
//           ]
//         },
//         {
//           title: 'Do you own your home?',
//           type: 'buttons',
//           field: 'homeOwner',
//           options: [
//             'Yes',
//             'No'
//           ]
//         }
//       ];

//       plumbing:any= [
//         {
//           title: 'What do you need help with?',
//           type: 'buttons',
//           field: 'projectType',
//           options: [
//            'Full residential repipe','Pex pipe','Copper repipe',
//            'Bathtubs','commercial plumbing','Drain cleaning','Faucets','Gas pipes',
//            'General repair','Leak repair','Sewer line repair',
//            'shower','sump pump','Toilets','Water heaters','Other'

//           ]
//         },
       
//         {
//           title: 'Do you own your home?',
//           type: 'buttons',
//           field: 'homeOwner',
//           options: [
//             'Yes',
//             'No'
//           ]
//         }
//       ];

//       door:any= [
//         {
//           title: 'what best describes your project?',
//           type: 'buttons',
//           field: 'projectType',
//           options: [
//             'Replace Door',
//             'Repair Door',
//             'Install New Door'
//           ]
//         },
//         {
//           title: 'What type of door project is this?',
//           type: 'buttons',
//           field: 'doorCount',
//           options: [
//            'exterior door',
//            'interior door',
//            'garage door',
//            'sliding door',
//            'storm door',
//            'security door',
//            'glass shower door',
//           ]
//         },
//         {
//           title: 'Do you own your home?',
//           type: 'buttons',
//           field: 'homeOwner',
//           options: [
//             'Yes',
//             'No'
//           ]
//         }
//       ];

//       flooring:any= [
//         {
//           title: 'what do you need help with?',
//           type: 'buttons',
//           field: 'projectType',
//           options: [
//             'Replace Flooring',
//             'Repair Flooring',
//             'Install New Flooring'
//           ]
//         },
//         {
//           title: 'What type of flooring material do you need?',
//           type: 'buttons',
//           field: 'flooringType',
//           options: [
//             'Vinyl',
//             'Woodern',
//             'carpet',
//             'Tile',
//             'Laminate',
//             'Concrete',
//             'stone',
//             'composite',
//             'Other'
//           ]
//         },
//          {
//           title: 'Do you own your home?',
//           type: 'buttons',
//           field: 'homeOwner',
//           options: [
//             'Yes',
//             'No'
//           ]
//         }
//       ];
        
//       gutter:any= [
//         {
//           title: 'what do you need help with?',
//           type: 'buttons',
//           field: 'projectType',
//           options: [
//             'Replace gutter',
//             'Repair gutter',
//             'Install New gutter',
//             'Close'
//           ]
//         },
//         {
//           title: 'What type of gutter material do you need?',
//           type: 'buttons',
//           field: 'gutterType',
//           options: [
//             'copper',
//             'Galvanized steel',
//             'PVC',
//             'Wood','seamless metal','Other'
//           ]
//         },
//          {
//           title: 'Do you own your home?',
//           type: 'buttons',
//           field: 'homeOwner',
//           options: [
//             'Yes',
//             'No'
//           ]
//         }
//       ];

//       fencing:any= [
//         {
//           title: 'what is the nature of your project?',
//           type: 'buttons',
//           field: 'projectType',
//           options: [
//             'Replace Fencing',
//             'Repair Fencing',
//             'Install New Fencing'
//           ]
//         },
//         {
//           title: 'What type of fencing material do you need?',
//           type: 'buttons',
//           field: 'fencingType',
//           options: [
//             'Wood',
//             'pvc',
//             'Vinyl',
//             'Chain link',
//             'Wrought iron',
//             'Alluminum',
//             'Barbed wire',
//             'Electric pet fence',
//             'Other'
//           ]
//         },
//          {
//           title: 'Do you own your home?',
//           type: 'buttons',
//           field: 'homeOwner',
//           options: [
//             'Yes',
//             'No'
//           ]
//         }
//       ];

//       homesecurity:any= [
//         {
//           title: 'what do you need help with?',
//           type: 'buttons',
//           field: 'projectType',
//           options: [
//             'Maintenance and Repair',
//             'Install New home security system',
//           ]
//         },
//         {
//           title: 'What type of home security system do you need?',
//           type: 'buttons',
//           field: 'securityType',
//           options: [
//             'Security Cameras',
//             'fire alarm system',
//             'panic buttons',
//             'Week leak detector',
//           ]
//         },
//          {
//           title: 'Do you own your home?',
//           type: 'buttons',
//           field: 'homeOwner',
//           options: [
//             'Yes',
//             'No'
//           ]
//         }
//       ]



// }
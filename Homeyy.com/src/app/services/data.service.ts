import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class DataService {

    heroContent: any = {
        roofing: {
            title: `Get Free Quotes from Top <br>Local Roofing Contractors `,
            description: `We have protected over 100 homes in your city over last 2 years. Free inspection — same-day response`,
            buttonText: 'Get Free Affordable Quotes',
            backgroundImage: '../../../../assets/images/banners/roofing_banner.jpg'
        },

        bathroom: {
            title: `Get Free Quotes for your <br>Bathroom Remodelling `,
            description: `Contractors, designer-grade materials, your project stays on time and on budget.`,
            buttonText: 'Get Free Affordable Quotes',
            backgroundImage: '../../../../assets/images/banners/Bathroom.jpg'
        },

        window: {
            title: `Get Free Quotes for your <br>Windows Remodelling `,
            description: `Energy Saving products, professional installation, that transfers when you sell your home.`,
            buttonText: 'Get Free Affordable Quotes',
            backgroundImage: '../../../../assets/images/banners/window.jpg'
        },

        hvac: {
            title: `Get Free Quotes from Top <br>Local HVAC Contractors `,
            description: `We have protected over 100 homes in your city over last 2 years. Free inspection — same-day response.`,
            buttonText: 'Get Free Affordable Quotes',
            backgroundImage: '../../../../assets/images/banners/hvac.jpg'
        },

        siding: {
            title: `Get Free Quotes from <br>Siding Remodelling `,
            description: `We have protected over 100 homes in your city over last 2 years. Free inspection — same-day response.`,
            buttonText: 'Get Free Affordable Quotes',
            backgroundImage: '../../../../assets/images/banners/siding.jpg'
        },

        plumbing: {
            title: `Get Free Quotes from <br>Local Plumbing Contractors `,
            description: `Homeyy has served 200+ homes and businesses across your city. Master plumbers, flat-rate pricing, we fix it right the first time or come back for free. `,
            buttonText: 'Get Free Affordable Quotes',
            backgroundImage: '../../../../assets/images/banners/plumbing.jpg'
        },

        door: {
            title: `Get Free Quotes from <br>Top Local Contractors `,
            description: `Homeyy has installed and replaced 200+ doors across your city. From entry doors to patio sliders.`,
            buttonText: 'Get My Free Door Estimate',
            backgroundImage: '../../../../assets/images/banners/door.jpg'
        },

        flooring: {
            title: `Get Free Quotes from <br> Top Local Flooring Contractors `,
            description: `Homeyy has installed 200+ flooring projects across your city. Hardwood, LVP, tile, carpet, and more - Flooring specialists, fixed-price quotes, and installation on every job. `,
            buttonText: 'Get Free Affordable Quotes',
            backgroundImage: '../../../../assets/images/banners/flooring.jpg'
        },

        gutter: {
            title: `Get Free Quotes from <br> Top Local Gutter Contractors `,
            description: `Homeyy has protected 200+ homes across your city. Seamless gutters, gutter guards, cleaning, and repair and same-week service on most jobs. `,
            buttonText: 'Get Free Gutter Estimate ',
            backgroundImage: '../../../../assets/images/banners/gutter.jpg'
        },

        fencing: {
            title: `Get Free Quotes from <br> Top Local Fencing Contractors `,
            description: `Homeyy has installed 100+ fences across your city. Wood, vinyl, aluminium, and chain link — as per project requirement.`,
            buttonText: 'Get My Free Fence Estimate',
            backgroundImage: '../../../../assets/images/banners/fencing.jpg'
        },

        solar: {
            title: `Get Free Quotes from <br>Top Local Solar Contractors `,
            description: `Homeyy has installed 150+ solar systems across your city. We handle, utility connection, and everything in between.`,
            buttonText: 'Get My Free Solar Assessment',
            backgroundImage: '../../../../assets/images/banners/solar.jpg'
        },

        kitchen: {
            title: `Get Free Quotes from <br> Top Local Kitchen Contractors `,
            description: `Homeyy has transformed 150+ kitchens across your city. From cabinet replacements to full gut renovations — fixed pricing, designers, fast-delivery`,
            buttonText: 'Get My Free Kitchen Consultation',
            backgroundImage: '../../../../assets/images/banners/kitchen.jpg'
        },

        homesecurity: {
            title: `Protect Your Home 24/7 <br>with Smart Security Systems <br>Installed Today.`,
            description: `Homeyy has protected 150+ homes across your city. Professional security system installation, smart home integration, and 24/7 professional monitoring — no long-term contracts, no hidden fees.`,
            buttonText: 'Get My Free Security Assessment ',
            backgroundImage: '../../../../assets/images/banners/security.jpg'
        }

    };

    // *\----------------------------- feature-strip -------------------------------\*

    features = [
        {
            img: '../../../../assets/images/home/home_01.svg',
            line1: 'Compare',
            line2: 'Quotations'
        },
        {
            img: '../../../../assets/images/home/home_03.svg',
            line1: 'Professional',
            line2: 'Expertise'
        },
        {
            img: '../../../../assets/images/home/home_02.svg',
            line1: 'Same-Day',
            line2: 'Response'
        }
    ];

    // *\----------------------------- About-Us ------------------------------------\*

    aboutContent: any = {

        roofing: {
            tag: 'ABOUT US',
            title: 'Reliable Roofing Services You Can Trust',
            description: 'We deliver reliable roofing solutions for everything from small repairs to major installations.',
            image: '../../../../assets/images/services/about/roofing.jpg',
            experience: '25+',
            experienceText: 'Year Of <br> Experience',
            buttonText: 'About Us',
            phone: '1-555-111-111',
            features: [
                {
                    icon: 'fa-solid fa-award',
                    title: 'Find top contractors',
                    description: 'We make it easy to find top and prescreened roofing installation contractors near you.'
                },
                {
                    icon: 'fa-solid fa-hand-holding-dollar',
                    title: 'Compare Prices',
                    description:
                        'The easiest way to get bids from several roofing installation contractors.'
                },
                {
                    icon: 'fa-solid fa-hourglass-half',
                    title: 'Increase Home Value',
                    description:
                        'Improve your curb appeal and invest in your home with premium roofing installation services.'
                },
                {
                    icon: 'fa-solid fa-headset',
                    title: '100% Free Quote',
                    description:
                        'Get free quotes from several of the top roofing installation contractors in your city.'
                }
            ]
        },

        bathroom: {
            tag: 'ABOUT US',
            title: 'Reliable Bathroom Services You Can Trust',
            description: 'We deliver reliable bathroom solutions for everything from small repairs to major installations.',
            image: '../../../../assets/images/services/about/bathroom.jpg',
            experience: '25+',
            experienceText: 'Year Of <br> Experience',
            buttonText: 'About Us',
            phone: '1-555-678-888',
            features: [
                {
                    icon: 'fa-solid fa-award',
                    title: 'Find top contractors',
                    description: 'We make it easy to find top and prescreened bathroom remodelling contractors near you.'
                },
                {
                    icon: 'fa-solid fa-hand-holding-dollar',
                    title: 'Compare Prices',
                    description:
                        'The easiest way to get bids from several bathroom remodelling contractors.'
                },
                {
                    icon: 'fa-solid fa-hourglass-half',
                    title: 'Increase Home Value',
                    description:
                        'Improve your curb appeal and invest in your home with premium bathroom remodelling services.'
                },
                {
                    icon: 'fa-solid fa-headset',
                    title: '100% Free Quote',
                    description:
                        'Get free quotes from several of the top bathroom remodelling contractors in your city.'
                }
            ]
        },

        window: {
            tag: 'ABOUT US',
            title: 'Reliable Window Services You Can Trust',
            description: 'We deliver reliable window solutions for everything from small repairs to major installations.',
            image: '../../../../assets/images/services/about/window.jpg',
            experience: '20+',
            experienceText: 'Year Of <br> Experience',
            buttonText: 'About Us',
            phone: '1-555-222-333',
            features: [
                {
                    icon: 'fa-solid fa-award',
                    title: 'Find top contractors',
                    description: 'We make it easy to find top and prescreened window installation contractors near you.'
                },
                {
                    icon: 'fa-solid fa-hand-holding-dollar',
                    title: 'Compare Prices',
                    description:
                        'The easiest way to get bids from several window installation contractors.'
                },
                {
                    icon: 'fa-solid fa-hourglass-half',
                    title: 'Increase Home Value',
                    description:
                        'Improve your curb appeal and invest in your home with premium window installation services.'
                },
                {
                    icon: 'fa-solid fa-headset',
                    title: '100% Free Quote',
                    description:
                        'Get free quotes from several of the top window installation contractors in your city.'
                }
            ]
        },

        hvac: {
            tag: 'ABOUT US',
            title: 'Reliable HVAC Services You Can Trust',
            description: 'We deliver reliable hvac solutions for everything from small repairs to major installations.',
            image: '../../../../assets/images/services/about/hvac.jpg',
            experience: '25+',
            experienceText: 'Year Of <br> Experience',
            buttonText: 'About Us',
            phone: '1-555-678-888',
            features: [
                {
                    icon: 'fa-solid fa-award',
                    title: 'Find top contractors',
                    description: 'We make it easy to find top and prescreened HVAC contractors near you.'
                },
                {
                    icon: 'fa-solid fa-hand-holding-dollar',
                    title: 'Compare Prices',
                    description:
                        'The easiest way to get bids from several HVAC contractors.'
                },
                {
                    icon: 'fa-solid fa-hourglass-half',
                    title: 'Increase Home Value',
                    description:
                        'Improve your curb appeal and invest in your home with HVAC services.'
                },
                {
                    icon: 'fa-solid fa-headset',
                    title: '100% Free Quote',
                    description:
                        'Get free quotes from several of the top HVAC contractors in your city.'
                }
            ]
        },

        siding: {
            tag: 'ABOUT US',
            title: 'Reliable Siding Services You Can Trust',
            description: 'We deliver reliable siding solutions for everything from small repairs to major installations.',
            image: '../../../../assets/images/services/about/siding.jpg',
            experience: '25+',
            experienceText: 'Year Of <br> Experience',
            buttonText: 'About Us',
            phone: '1-555-678-888',
            features: [
                {
                    icon: 'fa-solid fa-award',
                    title: 'Find top contractors',
                    description: 'We make it easy to find top and prescreened siding installation contractors near you.'
                },
                {
                    icon: 'fa-solid fa-hand-holding-dollar',
                    title: 'Compare Prices',
                    description:
                        'The easiest way to get bids from several siding installation contractors.'
                },
                {
                    icon: 'fa-solid fa-hourglass-half',
                    title: 'Increase Home Value',
                    description:
                        'Improve your curb appeal and invest in your home with premium siding installation services.'
                },
                {
                    icon: 'fa-solid fa-headset',
                    title: '100% Free Quote',
                    description:
                        'Get free quotes from several of the top siding installation contractors in your city.'
                }
            ]
        },

        kitchen: {
            tag: 'ABOUT US',
            title: 'Reliable Kitchen Services You Can Trust',
            description: 'We deliver reliable kitchen solutions for everything from small repairs to major installations.',
            image: '../../../../assets/images/services/about/kitchen.jpg',
            experience: '25+',
            experienceText: 'Year Of <br> Experience',
            buttonText: 'About Us',
            phone: '1-555-678-888',
            features: [
                {
                    icon: 'fa-solid fa-award',
                    title: 'Find top contractors',
                    description: 'We make it easy to find top and prescreened kitchen modeling contractors near you.'
                },
                {
                    icon: 'fa-solid fa-hand-holding-dollar',
                    title: 'Compare Prices',
                    description:
                        'The easiest way to get bids from several kitchen modeling contractors.'
                },
                {
                    icon: 'fa-solid fa-hourglass-half',
                    title: 'Increase Home Value',
                    description:
                        'Improve your curb appeal and invest in your home with premium kitchen modeling serices.'
                },
                {
                    icon: 'fa-solid fa-headset',
                    title: '100% Free Quote',
                    description:
                        'Get free quotes from several of the top kitchen modeling contractors in your city.'
                }
            ]
        },

        plumbing: {
            tag: 'ABOUT US',
            title: 'Reliable Plumbing Services You Can Trust',
            description: 'We deliver reliable plumbing solutions for everything from small repairs to major installations.',
            image: '../../../../assets/images/services/about/plumbing.jpg',
            experience: '25+',
            experienceText: 'Year Of <br> Experience',
            buttonText: 'About Us',
            phone: '1-555-678-888',
            features: [
                {
                    icon: 'fa-solid fa-award',
                    title: 'Find top contractors',
                    description: 'We make it easy to find top and prescreened plumbing contractors near you.'
                },
                {
                    icon: 'fa-solid fa-hand-holding-dollar',
                    title: 'Compare Prices',
                    description:
                        'The easiest way to get bids from several plumbing contractors'
                },
                {
                    icon: 'fa-solid fa-hourglass-half',
                    title: 'Increase Home Value',
                    description:
                        'Improve your curb appeal and invest in your home with premium plumbing services.'
                },
                {
                    icon: 'fa-solid fa-headset',
                    title: '100% Free Quote',
                    description:
                        'Get free quotes from several of the top plumbing contractors in your city.'
                }
            ]
        },

        door: {
            tag: 'ABOUT US',
            title: 'Reliable Door Services You Can Trust',
            description: 'We deliver reliable door solutions for everything from small repairs to major installations.',
            image: '../../../../assets/images/services/about/door.jpg',
            experience: '25+',
            experienceText: 'Year Of <br> Experience',
            buttonText: 'About Us',
            phone: '1-555-678-888',
            features: [
                {
                    icon: 'fa-solid fa-award',
                    title: 'Find top contractors',
                    description: 'We make it easy to find top and prescreened door installation contractors near you.'
                },
                {
                    icon: 'fa-solid fa-hand-holding-dollar',
                    title: 'Compare Prices',
                    description:
                        'The easiest way to get bids from several door installation contractors.'
                },
                {
                    icon: 'fa-solid fa-hourglass-half',
                    title: 'Increase Home Value',
                    description:
                        'Improve your curb appeal and invest in your home with premium door installation services.'
                },
                {
                    icon: 'fa-solid fa-headset',
                    title: '100% Free Quote',
                    description:
                        'Get free quotes from several of the top door installation contractors in your city.'
                }
            ]
        },

        flooring: {
            tag: 'ABOUT US',
            title: 'Reliable Flooring Services You Can Trust',
            description: 'We deliver reliable flooring solutions for everything from small repairs to major installations.',
            image: '../../../../assets/images/services/about/flooring.jpg',
            experience: '25+',
            experienceText: 'Year Of <br> Experience',
            buttonText: 'About Us',
            phone: '1-555-678-888',
            features: [
                {
                    icon: 'fa-solid fa-award',
                    title: 'Find top contractors',
                    description: 'We make it easy to find top and prescreened flooring contractors near you.'
                },
                {
                    icon: 'fa-solid fa-hand-holding-dollar',
                    title: 'Compare Prices',
                    description:
                        'The easiest way to get bids from several flooring contractors.'
                },
                {
                    icon: 'fa-solid fa-hourglass-half',
                    title: 'Increase Home Value',
                    description:
                        'Improve your curb appeal and invest in your home with premium flooring services.'
                },
                {
                    icon: 'fa-solid fa-headset',
                    title: '100% Free Quote',
                    description:
                        'Get free quotes from several of the top flooring contractors in your city.'
                }
            ]
        },

        gutter: {
            tag: 'ABOUT US',
            title: 'Reliable Gutter Services You Can Trust',
            description: 'We deliver reliable gutter solutions for everything from small repairs to major installations.',
            image: '../../../../assets/images/services/about/gutter.jpg',
            experience: '25+',
            experienceText: 'Year Of <br> Experience',
            buttonText: 'About Us',
            phone: '1-555-678-888',
            features: [
                {
                    icon: 'fa-solid fa-award',
                    title: 'Find top contractors',
                    description: 'We make it easy to find top and prescreened gutter installation contractors near you.'
                },
                {
                    icon: 'fa-solid fa-hand-holding-dollar',
                    title: 'Compare Prices',
                    description:
                        'The easiest way to get bids from several gutter installation contractors.'
                },
                {
                    icon: 'fa-solid fa-hourglass-half',
                    title: 'Increase Home Value',
                    description:
                        'Improve your curb appeal and invest in your home with premium gutter installation services.'
                },
                {
                    icon: 'fa-solid fa-headset',
                    title: '100% Free Quote',
                    description:
                        'Get free quotes from several of the top gutter installation contractors in your city.'
                }
            ]
        },

        fencing: {
            tag: 'ABOUT US',
            title: 'Reliable Fencing Services You Can Trust',
            description: 'We deliver reliable fencing solutions for everything from small repairs to major installations.',
            image: '../../../../assets/images/services/about/fencing.jpg',
            experience: '25+',
            experienceText: 'Year Of <br> Experience',
            buttonText: 'About Us',
            phone: '1-555-678-888',
            features: [
                {
                    icon: 'fa-solid fa-award',
                    title: 'Find top contractors',
                    description: 'We make it easy to find top and prescreened fencing installation contractors near you.'
                },
                {
                    icon: 'fa-solid fa-hand-holding-dollar',
                    title: 'Compare Prices',
                    description:
                        'The easiest way to get bids from several fencing installation contractors.'
                },
                {
                    icon: 'fa-solid fa-hourglass-half',
                    title: 'Increase Home Value',
                    description:
                        'Improve your curb appeal and invest in your home with premium fencing installation services.'
                },
                {
                    icon: 'fa-solid fa-headset',
                    title: '100% Free Quote',
                    description:
                        'Get free quotes from several of the top fencing installation contractors in your city.'
                }
            ]
        },

        solar: {
            tag: 'ABOUT US',
            title: 'Reliable Solar Services You Can Trust',
            description: 'We deliver reliable solar solutions for everything from small repairs to major installations.',
            image: '../../../../assets/images/services/about/solar.jpg',
            experience: '25+',
            experienceText: 'Year Of <br> Experience',
            buttonText: 'About Us',
            phone: '1-555-678-888',
            features: [
                {
                    icon: 'fa-solid fa-award',
                    title: 'Find top contractors',
                    description: 'We make it easy to find top and prescreened solar installation contractors near you.'
                },
                {
                    icon: 'fa-solid fa-hand-holding-dollar',
                    title: 'Compare Prices',
                    description:
                        'The easiest way to get bids from several solar installation contractors.'
                },
                {
                    icon: 'fa-solid fa-hourglass-half',
                    title: 'Increase Home Value',
                    description:
                        'Improve your curb appeal and invest in your home with premium solar installation services.'
                },
                {
                    icon: 'fa-solid fa-headset',
                    title: '100% Free Quote',
                    description:
                        'Get free quotes from several of the top solar installation contractors in your city.'
                }
            ]
        },

        homesecurity: {
            tag: 'ABOUT US',
            title: 'Reliable Home Security Services You Can Trust',
            description: 'We deliver reliable home security solutions for everything from small repairs to major installations.',
            image: '../../../../assets/images/services/about/security.jpg',
            experience: '25+',
            experienceText: 'Year Of <br> Experience',
            buttonText: 'About Us',
            phone: '1-555-678-888',
            features: [
                {
                    icon: 'fa-solid fa-award',
                    title: 'Find top contractors',
                    description: 'We make it easy to find top and prescreened home security contractors near you.'
                },
                {
                    icon: 'fa-solid fa-hand-holding-dollar',
                    title: 'Compare Prices',
                    description:
                        'The easiest way to get bids from several home security contractors.'
                },
                {
                    icon: 'fa-solid fa-hourglass-half',
                    title: 'Increase Home Value',
                    description:
                        'Improve your curb appeal and invest in your home with premium home security services.'
                },
                {
                    icon: 'fa-solid fa-headset',
                    title: '100% Free Quote',
                    description:
                        'Get free quotes from several of the top home security contractors in your city.'
                }
            ]
        },

    };

    // *\----------------------------- stats Content  ------------------------------\*

    statsContent: any = {

        roofing: [
            { target: 390, suffix: '+', title: 'Roofs Installed' },
            { target: 200, suffix: '+', title: 'Contractors all around USA' },
            { target: 50, suffix: '+', title: 'Cities served' },
            { target: 0, suffix: '$', title: 'Hidden fees' }
        ],

        bathroom: [
            { target: 290, suffix: '+', title: 'Bathrooms Remodeled' },
            { target: 200, suffix: '+', title: 'Contractors all around USA' },
            { target: 50, suffix: '+', title: 'Cities served' },
            { target: 0, suffix: '$', title: 'Hidden fees' }
        ],

        window: [
            { target: 300, suffix: '+', title: 'Windows Installed' },
            { target: 200, suffix: '+', title: 'Contractors all around USA' },
            { target: 50, suffix: '+', title: 'Cities served' },
            { target: 0, suffix: '$', title: 'Hidden fees' }
        ],

        hvac: [
            { target: 320, suffix: '+', title: 'HVAC Systems Serviced' },
            { target: 200, suffix: '+', title: 'Contractors all around USA' },
            { target: 50, suffix: '+', title: 'Cities served' },
            { target: 0, suffix: '$', title: 'Hidden fees' }
        ],

        siding: [
            { target: 260, suffix: '+', title: 'Homes Sided' },
            { target: 200, suffix: '+', title: 'Contractors all around USA' },
            { target: 50, suffix: '+', title: 'Cities served' },
            { target: 0, suffix: '$', title: 'Hidden fees' }
        ],

        plumbing: [
            { target: 350, suffix: '+', title: 'Jobs Completed' },
            { target: 200, suffix: '+', title: 'Contractors all around USA' },
            { target: 50, suffix: '+', title: 'Cities served' },
            { target: 0, suffix: '$', title: 'Hidden fees' }
        ],

        door: [
            { target: 320, suffix: '+', title: 'Doors Installed' },
            { target: 200, suffix: '+', title: 'Contractors all around USA' },
            { target: 50, suffix: '+', title: 'Cities served' },
            { target: 0, suffix: '$', title: 'Hidden fees' }
        ],

        flooring: [
            { target: 340, suffix: '+', title: 'Flooring Projects Completed' },
            { target: 200, suffix: '+', title: 'Contractors all around USA' },
            { target: 50, suffix: '+', title: 'Cities served' },
            { target: 0, suffix: '$', title: 'Hidden fees' }
        ],

        gutter: [
            { target: 280, suffix: '+', title: 'Homes Protected' },
            { target: 200, suffix: '+', title: 'Contractors all around USA' },
            { target: 50, suffix: '+', title: 'Cities served' },
            { target: 0, suffix: '$', title: 'Hidden fees' }
        ],

        fencing: [
            { target: 220, suffix: '+', title: 'Fences Installed' },
            { target: 200, suffix: '+', title: 'Contractors all around USA' },
            { target: 50, suffix: '+', title: 'Cities served' },
            { target: 0, suffix: '$', title: 'Hidden fees' }
        ],

        solar: [
            { target: 260, suffix: '+', title: 'Solar Systems Installed' },
            { target: 200, suffix: '+', title: 'Contractors all around USA' },
            { target: 50, suffix: '+', title: 'Cities served' },
            { target: 0, suffix: '$', title: 'Hidden fees' }
        ],

        homesecurity: [
            { target: 320, suffix: '+', title: 'Homes Protected' },
            { target: 200, suffix: '+', title: 'Contractors all around USA' },
            { target: 50, suffix: '+', title: 'Cities served' },
            { target: 0, suffix: '$', title: 'Hidden fees' }
        ],

        kitchen: [
            { target: 380, suffix: '+', title: 'Kitchens Remodeled' },
            { target: 200, suffix: '+', title: 'Contractors all around USA' },
            { target: 50, suffix: '+', title: 'Cities served' },
            { target: 0, suffix: '$', title: 'Hidden fees' }
        ]

    };

    // *\----------------------------- services Content  ----------------------------\*

    servicesContent: any = {

        roofing: {
            tag: 'SERVICES',
            title: 'Every Roof Need — One Trusted Team',
            description:
                ' From a single missing shingle to a full replacement, we handle it all with high quality materials that actually means something.',
            buttonText: 'View All Services',

            services: [
                {
                    title: 'Roof Repair',
                    image: '../../../../assets/images/services/roofing/01.jpg',
                    icon: 'fa-solid fa-faucet-drip',
                    description:
                        'Roof Repair Leaks, missing shingles, flashing failures - fixed fast before small damage becomes catastrophic. '
                },
                {
                    title: 'Roof Replacement',
                    image: '../../../../assets/images/services/roofing/02.jpg',
                    icon: 'fa-solid fa-toilet',
                    description:
                        'Full tear-off and replacement using premium shingles. Most jobs completed in one day.'
                },
                {
                    title: 'Roof Maintenance',
                    image: '../../../../assets/images/services/roofing/03.jpg',
                    icon: 'fa-solid fa-wrench',
                    description:
                        "Annual tune-ups that extend your roof's life by years."
                },

                {
                    title: 'Free Roof Inspection ',
                    image: '../../../../assets/images/services/roofing/05.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        '40-point inspection with photo documentation. No obligation. No pressure. Just facts.'
                },
                {
                    title: 'Leak Detection',
                    image: '../../../../assets/images/services/roofing/06.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'We find the source — not just the symptom. Pinpoint diagnosis before any work begins.'
                }
            ]
        },

        bathroom: {
            tag: 'SERVICES',
            title: 'Every Bathroom Service Under One Roof',
            description:
                'Whether you want a full gut-and-rebuild or a targeted upgrade that refreshes the space, we have the team, the trades, and the materials to make it happen — on time and on budget.',
            buttonText: 'View All Services',

            services: [
                {
                    title: 'Full Bathroom Remodel',
                    image: '../../../../assets/images/services/bathroom/01.jpg',
                    icon: 'fa-solid fa-faucet-drip',
                    description:
                        'Complete gut-and-rebuild — new layout, tile, fixtures, lighting, and finishes. Your vision expertly executed'
                },
                {
                    title: 'Walk-In Shower Conversion',
                    image: '../../../../assets/images/services/bathroom/02.jpg',
                    icon: 'fa-solid fa-toilet',
                    description:
                        'Replace your tub with a stunning custom walk-in shower. Most conversions completed in 3–5 days.'
                },
                {
                    title: 'Walk-In Tub Installation',
                    image: '../../../../assets/images/services/bathroom/03.jpg',
                    icon: 'fa-solid fa-wrench',
                    description:
                        "Safe, accessible soaking tubs with low-entry thresholds, jets, and grab bars. Ideal for aging-in-place."
                },
                {
                    title: 'Vanity & Countertop Upgrade',
                    image: '../../../../assets/images/services/bathroom/04.jpg',
                    icon: 'fa-solid fa-temperature-high',
                    description:
                        'New vanity, double sinks, quartz or marble countertops — a high-impact refresh without a full remodel.'
                },
                {
                    title: 'Tile & Flooring',
                    image: '../../../../assets/images/services/bathroom/05.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Porcelain, ceramic, stone, or LVP. Floors, walls, and shower surrounds — waterproofed and grouted perfectly.'
                },
                {
                    title: 'Fixture & Plumbing Updates',
                    image: '../../../../assets/images/services/bathroom/06.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'Faucets, showerheads, toilets, and lighting. Modernize your bath in a day without touching the walls.'
                },
                {
                    title: 'Accessibility Modifications',
                    image: '../../../../assets/images/services/bathroom/07.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Grab bars, roll-in showers, comfort-height toilets — code-compliant modifications for safety and independence.'
                },
                {
                    title: 'Free Design Consultation',
                    image: '../../../../assets/images/services/bathroom/08.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'In-home meeting with samples, 3D renderings, and a no-obligation estimate. See your new bathroom before we build it.'
                }
            ]
        },

        window: {
            tag: 'SERVICES',
            title: 'Every Window Type. Every Style. One Expert Team.',
            description:
                'From single broken panes to whole-home replacements, we carry every major window style and brand — with installation that keeps your home weather-tight.',
            buttonText: 'View All Services',

            services: [
                {
                    title: 'Window Replacement',
                    image: '../../../../assets/images/services/window/01.jpeg',
                    icon: 'fa-solid fa-faucet-drip',
                    description:
                        'Full single or multi-window replacement. Energy-efficient, custom-fit, installed in a single day for most homes.'
                },
                {
                    title: 'Window Repair',
                    image: '../../../../assets/images/services/window/02.jpeg',
                    icon: 'fa-solid fa-toilet',
                    description:
                        'Fogged glass, broken seals, cracked frames, hardware failures — repaired without full replacement where possible.'
                },
                {
                    title: 'Storm Windows',
                    image: '../../../../assets/images/services/window/03.jpg',
                    icon: 'fa-solid fa-wrench',
                    description:
                        "Heavy-duty impact-resistant windows for storm protection. superior noise reduction."
                },
                {
                    title: 'Bay & Bow Windows',
                    image: '../../../../assets/images/services/window/04.jpg',
                    icon: 'fa-solid fa-temperature-high',
                    description:
                        'Expand your space, capture views, and flood rooms with natural light. Custom-built and installed to code.'
                },
                {
                    title: 'Egress Windows',
                    image: '../../../../assets/images/services/window/05.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Code-compliant basement egress windows — required for legal bedrooms and critical for fire safety.'
                },
                {
                    title: 'Sliding Glass Doors',
                    image: '../../../../assets/images/services/window/06.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'New sliding patio doors to match your replacement windows. Energy-efficient frames and secure locking systems.'
                }, {
                    title: 'Commercial Windows',
                    image: '../../../../assets/images/services/window/07.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Office, retail, and multi-family window replacement. Storefront glazing, curtain wall repairs, and bulk pricing.'
                },
                {
                    title: 'Free Energy Audit',
                    image: '../../../../assets/images/services/window/08.jpeg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        "We measure your current windows' performance and show you exactly how much you'll save — in writing."
                }
            ]
        },

        hvac: {
            tag: 'SERVICES',
            title: 'Heating, Cooling & Everything In Between',
            description:
                'One call handles it all. Whether your AC is struggling in July or your furnace quits in January, our technicians are ready — with fully stocked trucks so most repairs are done in a single visit.',
            buttonText: 'View All Services',

            services: [
                {
                    title: 'AC Repair',
                    image: '../../../../assets/images/services/hvac/01.jpg',
                    icon: 'fa-solid fa-faucet-drip',
                    description:
                        'Not cooling? Strange noises? We diagnose and fix all makes and models — same day, upfront price.'
                },
                {
                    title: 'Furnace Repair',
                    image: '../../../../assets/images/services/hvac/02.jpg',
                    icon: 'fa-solid fa-toilet',
                    description:
                        'No heat? Yellow flame? We restore warmth fast — 24/7 emergency service, all brands.'
                },
                {
                    title: 'HVAC Replacement',
                    image: '../../../../assets/images/services/hvac/03.jpg',
                    icon: 'fa-solid fa-wrench',
                    description:
                        "Full system upgrade with energy-efficient units. Same-week installation."
                },
                {
                    title: 'Heat Pump Services',
                    image: '../../../../assets/images/services/hvac/04.jpg',
                    icon: 'fa-solid fa-temperature-high',
                    description:
                        'Install, repair, or replace heat pumps. Year-round efficiency — one system heats and cools.'
                },
                {
                    title: 'Maintenance Plans',
                    image: '../../../../assets/images/services/hvac/05.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Annual tune-ups that prevent breakdowns, extend system life.'
                },
                {
                    title: 'Indoor Air Quality',
                    image: '../../../../assets/images/services/hvac/06.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'Air purifiers, humidifiers, UV systems, and duct cleaning for healthier home air.'
                }, {
                    title: 'Smart Thermostats',
                    image: '../../../../assets/images/services/hvac/07.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Nest, Ecobee, or Honeywell — installed and configured to cut energy bills by up to 23%. '
                },
                {
                    title: '24/7 Emergency Service',
                    image: '../../../../assets/images/services/hvac/08.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'HVAC failure at 2am? We answer every call. No after-hours surcharge for members.'
                }
            ]
        },

        siding: {
            tag: 'SERVICES',
            title: 'Complete Exterior Solutions — From Repair to Full Replacement',
            description:
                " Whether you're replacing storm-damaged panels, upgrading your entire exterior, or bundling siding with windows and gutters for maximum value, our crew delivers lasting results.",
            buttonText: 'View All Services',

            services: [
                {
                    title: 'Vinyl Siding ',
                    image: '../../../../assets/images/services/siding/01.avif',
                    icon: 'fa-solid fa-faucet-drip',
                    description:
                        'Cost-effective, low-maintenance, and available in 100+ colors.'
                },
                {
                    title: 'Fiber Cement Siding',
                    image: '../../../../assets/images/services/siding/02.jpg',
                    icon: 'fa-solid fa-toilet',
                    description:
                        'James Hardie and similar — fire-resistant, rot-proof, and virtually maintenance-free.'
                },
                {
                    title: 'Engineered Wood Siding',
                    image: '../../../../assets/images/services/siding/03.jpg',
                    icon: 'fa-solid fa-wrench',
                    description:
                        "LP SmartSide and similar — the look of real wood with superior durability. Impact-resistant, paintable."
                },
                {
                    title: 'Insulated Siding',
                    image: '../../../../assets/images/services/siding/04.jpg',
                    icon: 'fa-solid fa-temperature-high',
                    description:
                        'Foam-backed panels that eliminate thermal bridging, reduce drafts, and cut energy bills by up to 20%.'
                },
                {
                    title: 'Siding Repair',
                    image: '../../../../assets/images/services/siding/05.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Cracked, warped, or storm-damaged panels repaired with matched materials.'
                },
                {
                    title: 'Storm Damage Replacement',
                    image: '../../../../assets/images/services/siding/06.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'Fast response to hail and wind damage. We handle everything for you.'
                },
                {
                    title: 'Trim & Soffit / Fascia',
                    image: '../../../../assets/images/services/siding/07.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Aluminum or PVC trim, soffits, and fascia to complete the exterior. Color-matched to your new siding.'
                },
                {
                    title: 'Siding + Windows Bundle',
                    image: '../../../../assets/images/services/siding/08.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'Replace siding and windows in one project for maximum savings — bundled projects are 2–3x the value and our installers are already there.'
                }
            ]
        },

        plumbing: {
            tag: 'SERVICES',
            title: 'Every Plumbing Problem. One Expert Team.',
            description:
                " From a dripping tap to a full sewer line replacement — our plumbers arrive stocked, diagnose fast, and fix it right the first time.",
            buttonText: 'View All Services',

            services: [
                {
                    title: 'Emergency Plumbing',
                    image: '../../../../assets/images/services/plumbing/01.jpeg',
                    icon: 'fa-solid fa-faucet-drip',
                    description:
                        'Burst pipes, major leaks, sewage backups — 60-minute response, 24 hours a day, 365 days a year.'
                },
                {
                    title: 'Drain Cleaning',
                    image: '../../../../assets/images/services/plumbing/02.jpeg',
                    icon: 'fa-solid fa-toilet',
                    description:
                        'Blocked sinks, showers, toilets, and main lines. Hydro-jetting and camera inspection available.'
                },
                {
                    title: 'Water Heater Repair & Replacement',
                    image: '../../../../assets/images/services/plumbing/03.jpeg',
                    icon: 'fa-solid fa-wrench',
                    description:
                        "All brands, tank and tankless. Same-day repair or next-day installation with permit pulled."
                },
                {
                    title: 'Leak Detection',
                    image: '../../../../assets/images/services/plumbing/04.jpeg',
                    icon: 'fa-solid fa-temperature-high',
                    description:
                        'Pinpoint hidden leaks behind walls and under slabs using acoustic and thermal technology — no guesswork.'
                },
                {
                    title: 'Toilet Repair & Replace',
                    image: '../../../../assets/images/services/plumbing/05.jpeg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Running, leaking, or cracked toilets fixed or replaced. Wide range of models in stock.'
                },
                {
                    title: 'Pipe Repair & Repiping',
                    image: '../../../../assets/images/services/plumbing/06.jpeg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'Corroded, cracked, or burst pipes repaired or fully repiped. PVC, copper, and PEX — your choice.'
                },
                {
                    title: 'Sewer Line Services',
                    image: '../../../../assets/images/services/plumbing/07.jpeg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Sewer camera inspection, clearing, repair, and trenchless replacement.'
                },

            ]
        },

        door: {
            tag: 'SERVICES',
            title: 'Every Door Type. Every Style. Expert Installation.',
            description:
                " From a broken entry door to a full set of French patio doors — our carpenters measure, supply, and install every door type with precision fit and lifetime weatherproofing.",
            buttonText: 'View All Services',

            services: [
                {
                    title: 'Entry Door Replacement',
                    image: '../../../../assets/images/services/door/01.jpg',
                    icon: 'fa-solid fa-faucet-drip',
                    description:
                        'Steel, fibreglass, or wood front doors — measured, supplied, and installed. Frames inspected for rot on every job.'
                },
                {
                    title: 'Door Repair & Rehang',
                    image: '../../../../assets/images/services/door/02.jpg',
                    icon: 'fa-solid fa-toilet',
                    description:
                        'Sticking, dragging, or misaligned doors rehung and adjusted. Hardware replaced where needed.'
                },
                {
                    title: 'Patio & Sliding Doors',
                    image: '../../../../assets/images/services/door/03.jpg',
                    icon: 'fa-solid fa-wrench',
                    description:
                        "Smooth-gliding sliding glass doors and energy-efficient patio doors installed with full weatherproofing."
                },
                {
                    title: 'French Doors',
                    image: '../../../../assets/images/services/door/04.jpg',
                    icon: 'fa-solid fa-temperature-high',
                    description:
                        'Interior and exterior French doors. Custom sizing available, with or without sidelights.'
                },
                {
                    title: 'Storm Doors',
                    image: '../../../../assets/images/services/door/05.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Added protection and insulation. Retractable screens, self-closing hinges, and low-E glass options.'
                },
                {
                    title: 'Storm Damage Replacement',
                    image: '../../../../assets/images/services/door/06.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'Fast response to hail and wind damage. We handle everything for you.'
                },
                {
                    title: 'Garage Side Doors',
                    image: '../../../../assets/images/services/door/07.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Code-compliant fire or standard steel access doors between garage and living space.'
                },
                {
                    title: 'Security Door Upgrade',
                    image: '../../../../assets/images/services/door/08.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'Multi-point locking systems, reinforced frames, and impact glass — for entry doors that mean business.'
                },
                {
                    title: 'Free In-Home Estimate',
                    image: '../../../../assets/images/services/door/09.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'We measure, show you samples, and give you a fixed price — no obligation, no pressure.'
                }
            ]
        },

        flooring: {
            tag: 'SERVICES',
            title: 'Every Floor Type. Expert Installation. One Team.',
            description:
                "Whether you're replacing old carpet with hardwood, tiling a bathroom, or waterproofing a basement — our flooring installers deliver a perfect finish. ",
            buttonText: 'View All Services',

            services: [
                {
                    title: 'Hardwood Flooring',
                    image: '../../../../assets/images/services/flooring/01.jpg',
                    icon: 'fa-solid fa-faucet-drip',
                    description:
                        'Solid and engineered hardwood installed, sanded, stained, and finished. Custom colour matching available.'
                },
                {
                    title: 'LVP / Luxury Vinyl',
                    image: '../../../../assets/images/services/flooring/02.jpg',
                    icon: 'fa-solid fa-toilet',
                    description:
                        '100% waterproof, scratch-resistant, and click-lock installed. Perfect for kitchens, bathrooms, and high-traffic areas.'
                },
                {
                    title: 'Tile & Stone',
                    image: '../../../../assets/images/services/flooring/03.jpg',
                    icon: 'fa-solid fa-wrench',
                    description:
                        'Porcelain, ceramic, slate, and natural stone. Floors, walls, and showers — precision-levelled and grouted.'
                },
                {
                    title: 'Carpet Installation',
                    image: '../../../../assets/images/services/flooring/04.jpg',
                    icon: 'fa-solid fa-temperature-high',
                    description:
                        'Stain-resistant, plush, and patterned carpets — measured, cut, and stretched with no visible seams.'
                },
                {
                    title: 'Hardwood Refinishing',
                    image: '../../../../assets/images/services/flooring/05.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Restore dull, scratched hardwood without full replacement. Sand, stain, and finish — looks brand new in 2–3 days.'
                },
                {
                    title: 'Laminate Flooring',
                    image: '../../../../assets/images/services/flooring/06.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'Budget-friendly wood-look laminate installed fast. Durable, low-maintenance, and available in 50+ styles.'
                },
                {
                    title: 'Subfloor Repair',
                    image: '../../../../assets/images/services/flooring/07.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Squeaky, soft, or uneven subfloors repaired before installation. We fix the foundation first.'
                },
                {
                    title: 'Free In-Home Quote',
                    image: '../../../../assets/images/services/flooring/08.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'We come to you with samples, measure the space, and give you a full fixed-price quote on the spot.'
                }
            ]
        },

        gutter: {
            tag: 'SERVICES',
            title: 'Complete Gutter Solutions — Protection You Can See Working',
            description:
                "From a one-time clean to a full seamless gutter system with lifetime guards — we protect your home's foundation, siding, and landscaping from water damage with expert installation and same-week service.",
            buttonText: 'View All Services',

            services: [
                {
                    title: 'Gutter Cleaning',
                    image: '../../../../assets/images/services/gutter/01.jpg',
                    icon: 'fa-solid fa-faucet-drip',
                    description:
                        'Full flush of gutters and downspouts. Debris removed, flow tested, minor repairs noted and quoted.'
                },
                {
                    title: 'Seamless Gutter Install',
                    image: '../../../../assets/images/services/gutter/02.jpg',
                    icon: 'fa-solid fa-toilet',
                    description:
                        'Custom-cut seamless aluminium gutters — no seams, no leaks. Installed in one day in 30+ colours.'
                },
                {
                    title: 'Gutter Repair',
                    image: '../../../../assets/images/services/gutter/03.jpg',
                    icon: 'fa-solid fa-wrench',
                    description:
                        'Sagging, leaking, or pulling-away gutters reattached, resealed, and reinforced. Fast turnaround.'
                },
                {
                    title: 'Gutter Guards',
                    image: '../../../../assets/images/services/gutter/04.jpg',
                    icon: 'fa-solid fa-temperature-high',
                    description:
                        'Micro-mesh, reverse-curve, or foam guards — eliminate cleaning and prevent blockages permanently.'
                },
                {
                    title: 'Downspout Extensions',
                    image: '../../../../assets/images/services/gutter/05.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Direct water away from your foundation with buried drainage or surface extensions.'
                },
                {
                    title: 'Storm Damage Repair',
                    image: '../../../../assets/images/services/gutter/06.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'Bent, detached, or crushed gutters repaired or replaced fast.'
                },
                {
                    title: 'Fascia & Soffit Repair',
                    image: '../../../../assets/images/services/gutter/07.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Rotted or damaged fascia board replacement as part of gutter installation or standalone service.'
                },
                {
                    title: 'Free Gutter Inspection',
                    image: '../../../../assets/images/services/gutter/08.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        "Full gutter assessment with photo report. We show you exactly what's needed — no obligation."
                }
            ]
        },

        fencing: {
            tag: 'SERVICES',
            title: 'Every Fence Type. Expert Installation. One Team.',
            description:
                "From a simple chain link to a custom cedar privacy fence — our contractors handle design, permits, and installation with a fixed price that lasts years.",
            buttonText: 'View All Services',

            services: [
                {
                    title: 'Wood Fencing',
                    image: '../../../../assets/images/services/fencing/01.jpg',
                    icon: 'fa-solid fa-faucet-drip',
                    description:
                        'Cedar, pine, and treated wood privacy fences. Custom heights, horizontal or vertical boards, stain or paint.'
                },
                {
                    title: 'Vinyl Fencing',
                    image: '../../../../assets/images/services/fencing/02.jpg',
                    icon: 'fa-solid fa-toilet',
                    description:
                        'Low-maintenance, rot-proof, and UV-resistant vinyl in white, tan, or grey.'
                },
                {
                    title: 'Aluminium Fencing',
                    image: '../../../../assets/images/services/fencing/03.jpg',
                    icon: 'fa-solid fa-wrench',
                    description:
                        'Elegant, rust-proof aluminium in classic and ornamental styles. Pool-code compliant options available.'
                },
                {
                    title: 'Chain Link Fencing',
                    image: '../../../../assets/images/services/fencing/04.jpg',
                    icon: 'fa-solid fa-temperature-high',
                    description:
                        'Galvanised and vinyl-coated chain link for security, pets, and perimeter protection. Fast installation.'
                },
                {
                    title: 'Fence Repair',
                    image: '../../../../assets/images/services/fencing/05.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Leaning, broken, or storm-damaged fence sections repaired or replaced. Same-week turnaround.'
                },
                {
                    title: 'Gate Installation',
                    image: '../../../../assets/images/services/fencing/06.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'Custom single or double gates — walk, drive, and pool gates with heavy-duty self-closing hardware.'
                },
                {
                    title: 'Pool Fencing',
                    image: '../../../../assets/images/services/fencing/07.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Code-compliant pool safety fencing in aluminium, glass, or vinyl. Inspection-ready on completion.'
                },
                {
                    title: 'Free Fence Estimate',
                    image: '../../../../assets/images/services/fencing/08.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'On-site measure, property line review, and a fixed-price written quote. No obligation.'
                }
            ]
        },

        solar: {
            tag: 'SERVICES',
            title: 'Everything You Need to Go Solar — Under One Roof',
            description:
                "From your first energy assessment to the day your system powers on — we handle design, permits, installation, utility connection, and 25 years of performance monitoring. You just start saving.",
            buttonText: 'View All Services',

            services: [
                {
                    title: 'Residential Solar',
                    image: '../../../../assets/images/services/solar/01.jpg',
                    icon: 'fa-solid fa-faucet-drip',
                    description:
                        'Custom-designed rooftop solar systems for single-family homes.'
                },
                {
                    title: 'Battery Storage',
                    image: '../../../../assets/images/services/solar/02.jpg',
                    icon: 'fa-solid fa-toilet',
                    description:
                        'Tesla Powerwall and compatible systems — store your solar energy and use it at night or during outages.'
                },
                {
                    title: 'Commercial Solar',
                    image: '../../../../assets/images/services/solar/03.jpg',
                    icon: 'fa-solid fa-wrench',
                    description:
                        'Rooftop and ground-mount systems for businesses. Reduce operating costs for accelerated depreciation.'
                },
                {
                    title: 'Solar + Roof Bundle',
                    image: '../../../../assets/images/services/solar/04.jpg',
                    icon: 'fa-solid fa-temperature-high',
                    description:
                        'New solar-ready roof and solar system installed together. One crew, one project, maximum savings.'
                },
                {
                    title: 'Solar Panel Cleaning',
                    image: '../../../../assets/images/services/solar/05.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Dirty panels lose up to 25% efficiency. Professional cleaning restores full output — annually or on demand.'
                },
                {
                    title: 'System Monitoring & Repair',
                    image: '../../../../assets/images/services/solar/06.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        '24/7 performance monitoring with alerts. Rapid response repair service if output drops.'
                },
                {
                    title: 'EV Charger Installation',
                    image: '../../../../assets/images/services/solar/07.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Level 2 home EV charger installed alongside or separate from your solar system. Charge on free solar power.'
                },
                {
                    title: 'Free Energy Assessment',
                    image: '../../../../assets/images/services/solar/08.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'In-home audit of your energy use, roof condition, and savings projection. Includes exact tax credit calculation.'
                }
            ]
        },

        kitchen: {
            tag: 'SERVICES',
            title: 'Every Kitchen Service. One Expert Team.',
            description:
                "Whether you want to refresh the cabinets or gut the entire kitchen down to the studs, we have the in-house designers, tradespeople, and project managers to deliver it — on time and on budget.",
            buttonText: 'View All Services',

            services: [
                {
                    title: 'Full Kitchen Remodel',
                    image: '../../../../assets/images/services/kitchen/01.jpg',
                    icon: 'fa-solid fa-faucet-drip',
                    description:
                        'Complete gut-and-rebuild with new layout, cabinets, countertops, flooring, and lighting. Your vision, fully executed.'
                },
                {
                    title: 'Cabinet Refacing & Replacement',
                    image: '../../../../assets/images/services/kitchen/02.jpg',
                    icon: 'fa-solid fa-toilet',
                    description:
                        'New cabinet doors and hardware for a fraction of a full remodel cost. Transformed in days, not weeks.'
                },
                {
                    title: 'Countertop Installation',
                    image: '../../../../assets/images/services/kitchen/03.jpg',
                    icon: 'fa-solid fa-wrench',
                    description:
                        'Quartz, granite, marble, or butcher block. Measured, cut, and installed with precision by our in-house team.'
                },
                {
                    title: 'Kitchen Island Addition',
                    image: '../../../../assets/images/services/kitchen/04.jpg',
                    icon: 'fa-solid fa-temperature-high',
                    description:
                        'Custom islands for prep space, seating, and storage — designed to match your existing layout and style.'
                },
                {
                    title: 'Flooring',
                    image: '../../../../assets/images/services/kitchen/05.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Tile, LVP, hardwood, or stone. Waterproof, durable, and matched to your new kitchen design.'
                },
                {
                    title: 'Lighting & Electrical',
                    image: '../../../../assets/images/services/kitchen/06.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'Under-cabinet lighting, pendant fixtures, and recessed lighting — planned by our designers, installed by our electricians.'
                },
                {
                    title: 'Backsplash Installation',
                    image: '../../../../assets/images/services/kitchen/07.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Tile, subway, or mosaic — the finishing touch that ties the whole kitchen together.'
                },
                {
                    title: 'Free Design Consultation',
                    image: '../../../../assets/images/services/kitchen/08.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'In-home meeting with material samples, 3D renderings, and a no-obligation fixed-price quote.'
                }
            ]
        },

        homesecurity: {
            tag: 'SERVICES',
            title: 'Complete Home Security — From Assessment to Monitoring',
            description:
                "We design, install, and support your security system end-to-end. Cameras, alarms, smart locks, and monitoring — all integrated and working together from day one.",
            buttonText: 'View All Services',

            services: [
                {
                    title: 'Security System Install',
                    image: '../../../../assets/images/services/security/01.jpg',
                    icon: 'fa-solid fa-faucet-drip',
                    description:
                        'Full alarm system with door, window, and motion sensors — installed and configured by technicians in one day.'
                },
                {
                    title: 'CCTV Camera Installation',
                    image: '../../../../assets/images/services/security/02.jpg',
                    icon: 'fa-solid fa-toilet',
                    description:
                        '4K indoor and outdoor cameras with night vision, AI motion detection, and remote viewing via app.'
                },
                {
                    title: 'Smart Doorbell Cameras',
                    image: '../../../../assets/images/services/security/03.jpg',
                    icon: 'fa-solid fa-wrench',
                    description:
                        'Video doorbells with two-way audio, motion alerts, and package detection — works with your existing doorbell wiring.'
                },
                {
                    title: 'Smart Lock Installation',
                    image: '../../../../assets/images/services/security/04.jpg',
                    icon: 'fa-solid fa-temperature-high',
                    description:
                        'Keypad, fingerprint, or app-controlled smart locks. Eliminate physical keys and control access remotely.'
                },
                {
                    title: '24/7 Monitoring Setup',
                    image: '../../../../assets/images/services/security/05.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        'Connect to a professional monitoring centre that dispatches police, fire, or ambulance when alarms trigger.'
                },
                {
                    title: 'Smart Home Integration',
                    image: '../../../../assets/images/services/security/06.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'Link your security system to Alexa, Google Home, or Apple HomeKit. Arm, view, and control everything in one app.'
                },
                {
                    title: 'Security Assessment',
                    image: '../../../../assets/images/services/security/07.jpg',
                    icon: 'fa-solid fa-screwdriver-wrench',
                    description:
                        "Free vulnerability assessment of your home's entry points, blind spots, and lighting — with a written recommendation."
                },
                {
                    title: 'System Upgrade',
                    image: '../../../../assets/images/services/security/08.jpg',
                    icon: 'fa-solid fa-toolbox',
                    description:
                        'Upgrade older systems with modern cameras, smart sensors, and cloud storage without replacing everything.'
                }
            ]
        },

    };

    // *\----------------------------- marquee Content  -----------------------------\*

    marqueeContent: any = {

        roofing: [
            { title: 'Residential', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Commercial', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Solar-Ready Roofing', icon: 'icon fa-solid fa-screwdriver-wrench' },
            // { title: 'Insurance Claims', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'HOA & Multi-Family', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'New Construction', icon: 'icon fa-solid fa-screwdriver-wrench' }
        ],

        bathroom: [
            { title: 'Full Luxury Remodel', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Budget-Smart Refresh', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Aging-in-Place Solutions', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Guest Bath Makeover', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Rental Property Remodel', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Commercial Restrooms', icon: 'icon fa-solid fa-screwdriver-wrench' }
        ],

        window: [
            { title: 'Energy Efficiency Upgrade', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Full Home Replacement', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Storm Damage Replacement', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Historic & Specialty Windows', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Rental & Investment Property', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'New Construction', icon: 'icon fa-solid fa-screwdriver-wrench' }
        ],

        hvac: [
            { title: 'Residential Repair & Service', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'New System Installation', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Commercial HVAC', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Preventive Maintenance', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Energy Efficiency Upgrades', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'New Construction', icon: 'icon fa-solid fa-screwdriver-wrench' }
        ],

        siding: [
            { title: 'Fiber Cement / Premium', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'ComVinyl / Budget-Friendlymercial', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Storm Damage Claim', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Insulated / Energy Upgrade', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Full Exterior Bundle', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'HOA / Rental Properties', icon: 'icon fa-solid fa-screwdriver-wrench' }
        ],

        plumbing: [
            { title: 'Residential Service', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Commercial Plumbing', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Water Heater Upgrade', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Drain & Sewer', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'New Construction', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Emergency Repair', icon: 'icon fa-solid fa-screwdriver-wrench' }
        ],

        door: [
            { title: 'Curb Appeal Upgrade', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Security Replacement', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Energy Efficiency', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Storm Damage Replacement', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Patio & Sliding', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Commercial Doors', icon: 'icon fa-solid fa-screwdriver-wrench' }
        ],

        flooring: [
            { title: 'Full Home Flooring', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Single Room Upgrade', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Waterproof Flooring', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Hardwood Refinishing', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Rental Property Flooring', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Commercial Flooring', icon: 'icon fa-solid fa-screwdriver-wrench' }
        ],

        gutter: [
            { title: 'Full Seamless System', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Gutter Cleaning', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Gutter Guard Upgrade', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Storm Damage Replace', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Commercial Gutters', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Gutter + Downspout Drainage', icon: 'icon fa-solid fa-screwdriver-wrench' }
        ],

        fencing: [
            { title: 'Privacy Wood Fence', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Vinyl — Low Maintenance', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Pool Safety Fence', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Pet & Child Containment', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Commercial Security', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'HOA & Rental Property', icon: 'icon fa-solid fa-screwdriver-wrench' }
        ],

        solar: [
            { title: 'Full Solar System', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Solar + Battery', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: '$0 Down Solar Lease/PPA', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Commercial Solar', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Solar + EV Charging', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Solar + Roof Bundle', icon: 'icon fa-solid fa-screwdriver-wrench' }
        ],

        homesecurity: [
            { title: 'Complete Smart System', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Cameras Only', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Alarm + Monitoring', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Rental Property Security', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Small Business Security', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Security Upgrade', icon: 'icon fa-solid fa-screwdriver-wrench' }
        ],

        kitchen: [
            { title: 'Full Luxury Remodel', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Budget-Smart Refresh', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Open Concept Conversion', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'Aging-in-Place Kitchen', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'FullRental Property Upgrade', icon: 'icon fa-solid fa-screwdriver-wrench' },
            { title: 'New Construction Kitchen', icon: 'icon fa-solid fa-screwdriver-wrench' }
        ],

    };

    // *\----------------------------- slider Content  -------------------------------\*

    sliderContent: any = {

        roofing: {
            tag: 'SOLUTIONS',
            title: "Roofing Solutions Built for Your Property",

            services: [
                {
                    title: 'Residential',
                    img: '../../../../assets/images/services/roofing/07.jpg',
                    desc:
                        'Single-family homes, townhomes, and condos. Asphalt shingles, metal, tile, and flat roofs.'
                },
                {
                    title: 'Commercial',
                    img: '../../../../assets/images/services/roofing/09.jpg',
                    desc:
                        'Flat roofs, TPO, EPDM, and modified bitumen for offices, warehouses, and retail — minimal business disruption.'
                },
                {
                    title: 'Solar-Ready Roofing',
                    img: '../../../../assets/images/services/roofing/08.jpg',
                    desc:
                        'New roof? We install solar-compatible underlayment so your roof is panel-ready from day one.'
                },
                // {
                //     title: 'Insurance Claims',
                //     img: '../../../../assets/images/home/solutions_4.png',
                //     desc:
                //         'We work directly with your insurance company. Free damage documentation, adjuster meetings, and claims navigation — at no charge.'
                // },
                {
                    title: 'HOA & Multi-Family',
                    img: '../../../../assets/images/services/roofing/01.jpg',
                    desc: 'Apartment complexes and HOA communities. Bulk pricing, phased scheduling, and a single point of contact.'
                },
                {
                    title: 'New Construction',
                    img: '../../../../assets/images/services/roofing/02.jpg',
                    desc: 'Builder partnerships for new homes and developments. On-schedule delivery with no compromise on material quality.'
                }
            ]
        },

        bathroom: {
            tag: 'SOLUTIONS',
            title: "Bathroom Solutions Tailored to Your Situation",

            services: [
                {
                    title: 'Full Luxury Remodel',
                    img: '../../../../assets/images/services/bathroom/01.jpg',
                    desc:
                        'Master baths with heated floors, frameless glass, freestanding tubs, and custom tile work. Premium materials, flawless execution.'
                },
                {
                    title: 'Budget-Smart Refresh',
                    img: '../../../../assets/images/services/bathroom/02.jpg',
                    desc:
                        'Maximum visual impact for your budget. We identify the upgrades that deliver the biggest transformation per dollar.'
                },

                {
                    title: 'Guest Bath Makeover',
                    img: '../../../../assets/images/services/bathroom/05.jpg',
                    desc:
                        'Fast, affordable updates to secondary bathrooms — new vanity, tile, fixtures, and paint. Done in days.'
                },
                {
                    title: 'Rental Property Remodel',
                    img: '../../../../assets/images/services/bathroom/07.jpg',
                    desc: 'Durable, tenant-proof upgrades that increase rental value and reduce maintenance. Bulk pricing available.'
                },
                {
                    title: 'Commercial Restrooms',
                    img: '../../../../assets/images/services/bathroom/04.jpg',
                    desc: 'ADA-compliant commercial restroom build-outs and renovations for offices, clinics, and retail spaces.'
                },
                {
                    title: 'Aging-in-Place Solutions',
                    img: '../../../../assets/images/services/bathroom/06.jpg',
                    desc:
                        'Walk-in tubs, barrier-free showers, grab bars, and non-slip flooring. Safe, beautiful, and ADA-compliant.'
                },
            ]
        },

        window: {
            tag: 'SOLUTIONS',
            title: "Window Solutions Tailored to Your Situation",

            services: [
                {
                    title: 'Energy Efficiency Upgrade',
                    img: '../../../../assets/images/services/window/01.jpeg',
                    desc:
                        'Triple-pane or Low-E glass that cuts heating and cooling bills by up to 30%. ROI in 3–7 years.'
                },
                {
                    title: 'Full Home Replacement',
                    img: '../../../../assets/images/services/window/02.jpeg',
                    desc:
                        "Replace every window in a single project. Consistent look throughout, one installation crew visit."
                },
                {
                    title: 'Storm Damage Replacement',
                    img: '../../../../assets/images/services/window/03.jpg',
                    desc:
                        'Broken or damaged windows repaired or replaced fast. We assist with documentation and claims at no extra charge.'
                },
                {
                    title: 'Historic & Specialty Windows',
                    img: '../../../../assets/images/services/window/04.jpg',
                    desc:
                        'Wood, aluminum-clad, or custom profiles for older homes. We match existing architecture while upgrading performance.'
                },
                {
                    title: 'Rental & Investment Property',
                    img: '../../../../assets/images/services/window/05.jpg',
                    desc: 'Durable, low-maintenance windows that increase property value and rental appeal. Bulk and multi-unit pricing available.'
                },
                {
                    title: 'New Construction',
                    img: '../../../../assets/images/services/window/01.jpg',
                    desc: 'Builder partnerships for new homes. Code-compliant, on-schedule window installation with full documentation. '
                }
            ]
        },

        hvac: {
            tag: 'SOLUTIONS',
            title: "HVAC Solutions Tailored to Your Situation",

            services: [
                {
                    title: 'Residential Repair & Service',
                    img: '../../../../assets/images/services/hvac/02.jpg',
                    desc:
                        'Homeowners across your city. Same-day repairs, honest diagnostics, no-pressure options. We fix it right the first time.'
                },
                {
                    title: 'New System Installation',
                    img: '../../../../assets/images/services/hvac/01.jpg',
                    desc:
                        'Upgrading AC, furnace, or full HVAC. Energy-efficient units, Same-week installation by techs.'
                },
                {
                    title: 'Commercial HVAC',
                    img: '../../../../assets/images/services/hvac/03.jpg',
                    desc:
                        'Offices, retail, restaurants, and warehouses. Planned maintenance contracts and emergency response — minimal downtime.'
                },
                {
                    title: 'Preventive Maintenance',
                    img: '../../../../assets/images/services/hvac/05.jpg',
                    desc:
                        'Members get priority scheduling, 15% off repairs, and annual tune-ups that prevent 90% of emergency breakdowns.'
                },
                {
                    title: 'Energy Efficiency Upgrades',
                    img: '../../../../assets/images/services/hvac/08.jpg',
                    desc: 'High-SEER systems, heat pumps, smart thermostats, and insulation audits — lower bills.'
                },
                {
                    title: 'New Construction',
                    img: '../../../../assets/images/services/hvac/02.jpg',
                    desc: 'Builder and contractor partnerships. Code-compliant HVAC design, installation, and inspection — on schedule.'
                }
            ]
        },

        siding: {
            tag: 'SOLUTIONS',
            title: "Siding That Fits Your Home, Your Climate, and Your Budget",

            services: [
                {
                    title: 'Fiber Cement / Premium',
                    img: '../../../../assets/images/services/siding/05.jpg',
                    desc:
                        'For homeowners who want rot-proof, fire-resistant. The last siding your home will ever need.'
                },
                {
                    title: 'Vinyl / Budget-Friendly',
                    img: '../../../../assets/images/services/siding/10.jpg',
                    desc:
                        'The most affordable path to a dramatically improved exterior. 100+ color options.'
                },
                {
                    title: 'Insulated / Energy Upgrade',
                    img: '../../../../assets/images/services/siding/12.jpg',
                    desc:
                        'Foam-backed insulated siding reduces heat transfer and drafts. Pairs with new windows for whole-envelope energy efficiency.'
                },
                {
                    title: 'Full Exterior Bundle',
                    img: '../../../../assets/images/services/siding/13.jpg',
                    desc: 'Siding + windows + gutters replaced in one project. Maximum value, single crew, one point of contact from start to finish.'
                },
                {
                    title: 'HOA / Rental Properties',
                    img: '../../../../assets/images/services/siding/02.jpg',
                    desc: 'HOA-compliant color and material selection with documentation. Bulk pricing for rental portfolios and multi-unit properties.'
                }
            ]
        },

        plumbing: {
            tag: 'SOLUTIONS',
            title: "Plumbing Solutions for Every Situation",
            services: [
                {
                    title: 'Emergency Repair',
                    img: '../../../../assets/images/services/plumbing/01.jpeg',
                    desc:
                        'Flooding, burst pipes, sewage backup — we respond in 60 minutes any time of day or night. No extra after-hours charge.'
                },
                {
                    title: 'Residential Service',
                    img: '../../../../assets/images/services/plumbing/02.jpeg',
                    desc:
                        'Homes and apartments across your city. Repairs, upgrades, and full replumbing'
                },
                {
                    title: 'Commercial Plumbing',
                    img: '../../../../assets/images/services/plumbing/08.jpg',
                    desc:
                        'Restaurants, offices, and retail. Scheduled maintenance and emergency response — minimal business downtime.'
                },
                {
                    title: 'Water Heater Upgrade',
                    img: '../../../../assets/images/services/plumbing/03.jpeg',
                    desc:
                        'Tankless or high-efficiency tank systems. Lower energy bills, endless hot water.'
                },
                {
                    title: 'Drain & Sewer',
                    img: '../../../../assets/images/services/plumbing/07.jpeg',
                    desc: 'Hydro-jetting, camera inspection, and trenchless sewer repair. We fix root intrusion and blockages permanently.'
                },
                {
                    title: 'New Construction',
                    img: '../../../../assets/images/services/plumbing/05.jpeg',
                    desc: 'Rough-in and finish plumbing for new builds and additions. On-schedule, code-compliant, permit-managed.'
                }
            ]
        },

        door: {
            tag: 'SOLUTIONS',
            title: "Door Solutions for Every Home and Every Need",

            services: [
                {
                    title: 'Curb Appeal Upgrade',
                    img: '../../../../assets/images/services/door/01.jpg',
                    desc:
                        'New fibreglass or steel entry door in 50+ styles and colours with instant visual impact.'
                },
                {
                    title: 'Security Replacement',
                    img: '../../../../assets/images/services/door/08.jpg',
                    desc:
                        "Impact doors with multi-point locks and reinforced frames. Upgrade your home's first line of defence"
                },
                {
                    title: 'Energy Efficiency',
                    img: '../../../../assets/images/services/door/04.jpg',
                    desc:
                        "Triple-seal weatherstripping, insulated core doors, and low-E glass — stop drafts and cut heating bills."
                },

                {
                    title: 'Patio & Sliding',
                    img: '../../../../assets/images/services/door/03.jpg',
                    desc: 'New sliding or French patio doors to connect your indoor and outdoor living space seamlessly.'
                },
                {
                    title: 'Commercial Doors',
                    img: '../../../../assets/images/services/door/05.jpg',
                    desc: 'Office, retail, and warehouse entry doors. ADA-compliant options, heavy-duty hardware, and volume pricing.'
                }
            ]
        },

        flooring: {
            tag: 'SOLUTIONS',
            title: "Flooring Solutions for Every Room and Every Budget",

            services: [
                {
                    title: 'Full Home Flooring',
                    img: '../../../../assets/images/services/flooring/02.jpg',
                    desc:
                        'Replace every floor in your home in one project — Consistent look throughout, one crew visit.'
                },
                {
                    title: 'Single Room Upgrade',
                    img: '../../../../assets/images/services/flooring/06.jpg',
                    desc:
                        'Targeted replacement of one or two rooms. Fast turnaround, minimal disruption, matches your existing flooring where needed.'
                },
                {
                    title: 'Waterproof Flooring',
                    img: '../../../../assets/images/services/flooring/01.jpg',
                    desc:
                        'LVP or porcelain tile for kitchens, bathrooms, laundry, and basements — 100% waterproof, pet-proof, and kid-proof.'
                },
                {
                    title: 'Hardwood Refinishing',
                    img: '../../../../assets/images/services/flooring/05.jpg',
                    desc:
                        'Restore damaged or dull hardwood without replacement. 70% less than new floors — same dramatic transformation.'
                },
                {
                    title: 'Rental Property Flooring',
                    img: '../../../../assets/images/services/flooring/07.jpg',
                    desc: 'Durable, low-maintenance finishes that hold up to tenants and increase property value. Bulk pricing for portfolios.'
                },
                {
                    title: 'Commercial Flooring',
                    img: '../../../../assets/images/services/flooring/02.jpg',
                    desc: 'High-traffic commercial-grade LVP, tile, and carpet for offices, retail, and hospitality. Fast installation schedules.'
                }
            ]
        },

        gutter: {
            tag: 'SOLUTIONS',
            title: "Gutter Solutions for Every Home and Every Problem",

            services: [
                {
                    title: 'Full Seamless System',
                    img: '../../../../assets/images/services/gutter/02.jpg',
                    desc:
                        'New seamless gutters and guards installation. 30+ color options.'
                },
                {
                    title: 'Gutter Cleaning',
                    img: '../../../../assets/images/services/gutter/01.jpg',
                    desc:
                        'OOne-time or annual cleaning to keep existing gutters flowing. Debris removed, downspouts flushed, minor repairs quoted.'
                },
                {
                    title: 'Gutter Guard Upgrade',
                    img: '../../../../assets/images/services/gutter/04.jpg',
                    desc:
                        'Add guards to your existing gutters — eliminate annual cleaning and prevent blockages.',
                },

                {
                    title: 'Commercial Gutters',
                    img: '../../../../assets/images/services/gutter/08.jpg',
                    desc:
                        'Large-format commercial guttering for warehouses, retail, and multi-unit properties. High-volume and box gutter options. '
                },
                {
                    title: 'Gutter + Downspout Drainage',
                    img: '../../../../assets/images/services/gutter/05.jpg',
                    desc:
                        'Full water management system — gutters, guards, downspouts, and buried drainage to protect your foundation.'
                }
            ]
        },

        fencing: {
            tag: 'SOLUTIONS',
            title: "Fencing Solutions for Every Property and Every Purpose",

            services: [
                {
                    title: 'Privacy Wood Fence',
                    img: '../../../../assets/images/services/fencing/01.jpg',
                    desc:
                        "6ft cedar or treated wood privacy fence — solid panels, custom stain, and concrete-set posts that won't shift."
                },
                {
                    title: 'Vinyl — Low Maintenance',
                    img: '../../../../assets/images/services/fencing/02.jpg',
                    desc:
                        'Rot-proof, splinter-free vinyl that never needs painting. 30+ styles.'
                },
                {
                    title: 'Pool Safety Fence',
                    img: '../../../../assets/images/services/fencing/07.jpg',
                    desc:
                        'Code-compliant pool fencing in aluminium or glass. Self-closing, self-latching — passes inspection first time.'
                },
                {
                    title: 'Pet & Child Containment',
                    img: '../../../../assets/images/services/fencing/06.jpg',
                    desc:
                        'Full-perimeter fencing designed to keep pets in and strangers out. Solid panels, no-gap design.'
                },
                {
                    title: 'Commercial Security',
                    img: '../../../../assets/images/services/fencing/03.jpg',
                    desc:
                        'High-security chain link, palisade, or anti-climb fencing for commercial sites, schools, and warehouses.'
                },
                {
                    title: 'HOA & Rental Property',
                    img: '../../../../assets/images/services/fencing/08.jpg',
                    desc:
                        'HOA-compliant fence styles with documentation prepared. Bulk pricing for rental portfolios.'
                }
            ]
        },

        solar: {
            tag: 'SOLUTIONS',
            title: "Solar Solutions for Every Home, Budget, and Energy Goal",

            services: [
                {
                    title: 'Full Solar System',
                    img: '../../../../assets/images/services/solar/01.jpg',
                    desc:
                        'Custom-designed grid-tied solar system sized to offset your electricity bill.'
                },
                {
                    title: 'Solar + Battery',
                    img: '../../../../assets/images/services/solar/02.jpg',
                    desc:
                        'Full solar with Tesla Powerwall backup — stay powered during outages and maximise self-consumption.'
                },
                {
                    title: '$0 Down Solar Lease/PPA',
                    img: '../../../../assets/images/services/solar/01.jpg',
                    desc:
                        'No upfront cost, immediate savings. You pay only for the power your panels produce — at a lower rate than the grid.'
                },
                {
                    title: 'Commercial Solar',
                    img: '../../../../assets/images/services/solar/03.jpg',
                    desc:
                        'Large-format rooftop and ground arrays for businesses. ROI in 4–6 years, accelerated tax depreciation available.'
                },
                {
                    title: 'Solar + EV Charging',
                    img: '../../../../assets/images/services/solar/07.jpg',
                    desc:
                        'Power your car on sunshine. Level 2 charger installed alongside your solar system — charge overnight for free.'
                },
                {
                    title: 'Solar + Roof Bundle',
                    img: '../../../../assets/images/services/solar/04.jpg',
                    desc:
                        'Replace your aging roof and go solar in one project. Solar-ready materials, one installation team, maximum value.'
                }
            ]
        },

        homesecurity: {
            tag: 'SOLUTIONS',
            title: "Security Solutions for Every Home and Every Concern ",

            services: [
                {
                    title: 'Complete Smart System',
                    img: '../../../../assets/images/services/security/01.jpg',
                    desc:
                        'Full alarm + cameras + smart locks + monitoring. One installation, fully integrated, works with Alexa/Google/Apple.'
                },
                {
                    title: 'Cameras Only',
                    img: '../../../../assets/images/services/security/02.jpg',
                    desc:
                        '4K outdoor and indoor camera system with remote viewing. No alarm, no contract — just visibility and peace of mind.'
                },
                {
                    title: 'Alarm + Monitoring',
                    img: '../../../../assets/images/services/security/05.jpg',
                    desc:
                        'Professional alarm system with 24/7 monitoring centre. Police dispatched automatically when triggered — no monthly lock-in.'
                },
                {
                    title: 'Rental Property Security',
                    img: '../../../../assets/images/services/security/09.jpg',
                    desc:
                        'Remote-view cameras, smart locks with individual codes per tenant, and motion alerts. Manage multiple properties from one app.'
                },
                {
                    title: 'Small Business Security',
                    img: '../../../../assets/images/services/security/07.jpg',
                    desc:
                        'Cameras, alarms, access control, and monitoring for offices, retail, and warehouses. Installed after hours to avoid disruption.'
                },
                {
                    title: 'Security Upgrade',
                    img: '../../../../assets/images/services/security/10.jpg',
                    desc:
                        'Modernise your existing alarm system with 4K cameras, smart sensors, and app control. Keep existing infrastructure where possible.'
                }
            ]
        },

        kitchen: {
            tag: 'SOLUTIONS',
            title: "A Kitchen Remodel Built Around How You Actually Live ",

            services: [
                {
                    title: 'Full Luxury Remodel',
                    img: '../../../../assets/images/services/kitchen/09.jpg',
                    desc:
                        'Custom cabinetry, waterfall countertops, chef-grade fixtures, and designer lighting. Premium materials, flawless execution from concept to completion.'
                },
                {
                    title: 'Budget-Smart Refresh',
                    img: '../../../../assets/images/services/kitchen/01.jpg',
                    desc:
                        'Cabinet refacing, new countertops, and updated hardware for maximum visual impact without tearing everything out.'
                },
                {
                    title: 'Open Concept Conversion',
                    img: '../../../../assets/images/services/kitchen/08.jpg',
                    desc:
                        'Remove walls, extend the layout, and create the open kitchen-living space modern homes demand. Full structural and design work handled in-house.'
                },
                {
                    title: 'Aging-in-Place Kitchen',
                    img: '../../../../assets/images/services/kitchen/04.jpg',
                    desc:
                        'Lower countertops, pull-out shelving, lever handles, and touchless fixtures — functional, beautiful, and safe for every stage of life.'
                },
                {
                    title: 'Rental Property Upgrade',
                    img: '../../../../assets/images/services/kitchen/03.jpg',
                    desc:
                        'Durable, tenant-proof finishes that increase rental value and reduce maintenance. Bulk pricing available for portfolio landlords.'
                },
                {
                    title: 'New Construction Kitchen',
                    img: '../../../../assets/images/services/kitchen/01.jpg',
                    desc:
                        'Builder and contractor partnerships. On-schedule cabinet, countertop, and fixture installation with full documentation for inspections.'
                }
            ]
        },


    };

    // *\----------------------------- comparison Content  ---------------------------\*

    comparisonContent: any = {

        roofing: {
            tag: 'WHY HOMEYY',
            title: "We Know What's Keeping You Up at Night",

            features: [
                {
                    feature: 'Quote Turnaround',
                    others: '3–5 days',
                    homeyy: 'Same day'
                },
                {
                    feature: 'Response Time',
                    others: 'No guarantee',
                    homeyy: 'Within 2 hours'
                },
                {
                    feature: 'Pricing',
                    others: 'Estimate changes midway',
                    homeyy: 'Fixed, locked-in price'
                },
                {
                    feature: 'Project Timeline',
                    others: 'Open-ended',
                    homeyy: 'Completion date in writing'
                },
                {
                    feature: 'Crew',
                    others: 'Subcontractors',
                    homeyy: 'Top Local Contractors'
                },

                {
                    feature: 'Cleanup',
                    others: 'Next-day or skip',
                    homeyy: 'Same-day, zero mess'
                },
                // {
                //     feature: 'Certification',
                //     others: 'Unlisted / unknown',
                //     homeyy: 'GAF Master Elite'
                // }
            ]
        },

        bathroom: {
            tag: 'WHY HOMEYY',
            title: "Bathroom Renovations Have a Bad Reputation. We're Changing That.",

            features: [
                {
                    feature: 'Quote Turnaround',
                    others: '5–7 days',
                    homeyy: 'Same day'
                },
                {
                    feature: 'Renderings Before Work',
                    others: 'Not provided',
                    homeyy: '3D rendering included'
                },
                {
                    feature: 'Pricing',
                    others: 'Quote changes mid-project',
                    homeyy: 'ixed price — locked in writing '
                },
                // {
                //     feature: 'Project Duration',
                //     others: '3–6 weeks',
                //     homeyy: '5–15 business days average'
                // },
                {
                    feature: 'Hidden Damage Disclosure',
                    others: 'Billed as discovered',
                    homeyy: 'Pre-construction inspection — disclosed upfront'
                },
                {
                    feature: 'Daily Cleanup ',
                    others: 'End of project only',
                    homeyy: 'Every day, without exception'
                },

                // {
                //     feature: 'Financing',
                //     others: 'Not available',
                //     homeyy: '0% financing on full remodels'
                // },
            ]
        },

        window: {
            tag: 'WHY HOMEYY',
            title: "Window Shopping Is Exhausting. We Make It Simple.",

            features: [
                // {
                //     feature: 'Emergency Response',
                //     others: 'Next available (1–3 days)',
                //     homeyy: 'Within 90 minutes'
                // },
                {
                    feature: 'Quote Process',
                    others: 'Phone estimate only',
                    homeyy: 'Free on-site diagnostic'
                },
                {
                    feature: 'Pricing',
                    others: 'Surprise charges at invoice',
                    homeyy: 'Upfront flat-rate pricing'
                },
                {
                    feature: 'Cabinet Lead',
                    others: 'TimeSurprises after you sign',
                    homeyy: 'Confirmed before project starts'
                },
                {
                    feature: 'Repair vs Replace Advice',
                    others: 'Push replacement for margin',
                    homeyy: 'Honest cost comparison — your choice'
                },

                {
                    feature: 'Material Quality',
                    others: 'Builder-grade standard',
                    homeyy: 'Premium materials, always'
                },
            ]
        },

        hvac: {
            tag: 'WHY HOMEYY',
            title: "We Know What's Keeping You Up at Night",

            features: [
                // {
                //     feature: 'Emergency Response',
                //     others: 'Next available (1–3 days)',
                //     homeyy: 'Within 90 minutes'
                // },
                {
                    feature: 'Quote Process',
                    others: 'Phone estimate only',
                    homeyy: 'Free on-site diagnostic'
                },
                {
                    feature: 'Pricing',
                    others: 'Surprise charges at invoice',
                    homeyy: 'Upfront flat-rate pricing'
                },
                {
                    feature: 'Cabinet Lead',
                    others: 'TimeSurprises after you sign',
                    homeyy: 'Confirmed before project starts'
                },
                {
                    feature: 'Repair vs Replace Advice',
                    others: 'Push replacement for margin',
                    homeyy: 'Honest cost comparison — your choice'
                },

               {
                    feature: 'Material Quality',
                    others: 'Builder-grade standard',
                    homeyy: 'Premium materials, always'
                },
            ]
        },

        siding: {
            tag: 'WHY HOMEYY',
            title: "Siding That Fits Your Home, Your Climate, and Your Budget",

            features: [
                {
                    feature: 'Quote Turnaround',
                    others: '5–7 days',
                    homeyy: 'Same day'
                },
                {
                    feature: 'Material Recommendation',
                    others: 'Push highest-margin product ',
                    homeyy: 'Recommend based on your climate & budget'
                },

                {
                    feature: 'HOA Compliance Help',
                    others: 'Not offered',
                    homeyy: 'Full documentation & approval support'
                },
                {
                    feature: 'Bundling',
                    others: 'Siding only',
                    homeyy: 'Siding + windows + gutters in one project'
                },
                {
                    feature: 'Daily Site Cleanup',
                    others: 'End of project only',
                    homeyy: 'Every evening — yard always tidy'
                },
                {
                    feature: 'Cabinet Lead',
                    others: 'TimeSurprises after you sign',
                    homeyy: 'Confirmed before project starts'
                },
                {
                    feature: 'Material Quality',
                    others: 'Builder-grade standard',
                    homeyy: 'Premium materials, always'
                },
            ]
        },

        plumbing: {
            tag: 'WHY HOMEYY',
            title: "What Sets Homeyy Apart Before We Even Pick Up a Tool ",

            features: [
                // {
                //     feature: 'Emergency Response',
                //     others: 'Next available slot (1–3 days)',
                //     homeyy: 'Within 60 minutes, 24/7'
                // },
                {
                    feature: 'Quote Process',
                    others: 'Phone estimate, changes on-site',
                    homeyy: 'Upfront flat-rate before work starts'
                },
                {
                    feature: 'SPricing Transparency',
                    others: 'Bill surprises at invoice',
                    homeyy: 'Fixed price — zero hidden fees'
                },

                {
                    feature: 'Drain Camera Inspection',
                    others: 'SidCharged separately',
                    homeyy: 'Included on complex job'
                },
                {
                    feature: 'After-Hours Service',
                    others: 'Extra surcharge',
                    homeyy: 'No surcharge — same rate'
                },
                {
                    feature: 'Job Completion',
                    others: 'Parts ordered, return visit',
                    homeyy: 'Fully stocked vans — 1-visit fix'
                },

                {
                    feature: 'Follow-Up',
                    others: 'None',
                    homeyy: 'Post-job check-in call'
                },
                // {
                //     feature: 'Financing',
                //     others: 'Not available',
                //     homeyy: 'Flexible payment options'
                // },
            ]
        },

        door: {
            tag: 'WHY HOMEYY',
            title: "Why Homeyy — Not the Other Guys",

            features: [
                {
                    feature: 'Quote Turnaround',
                    others: '3–5 days',
                    homeyy: 'Same day'
                },
                {
                    feature: 'Installation Timeline',
                    others: '2–3 weeks backlog',
                    homeyy: 'Same week available'
                },
                {
                    feature: 'Pricing',
                    others: 'Quote changes after measure',
                    homeyy: 'Fixed price after in-home measure'
                },
                {
                    feature: 'Door Selection',
                    others: 'Limited stock brands',
                    homeyy: '50+ styles, all major brands'
                },
                {
                    feature: 'Weatherproofing',
                    others: 'Basic seal included',
                    homeyy: 'Full weatherstripping + threshold'
                },
                {
                    feature: 'Frame Inspection',
                    others: 'Not included',
                    homeyy: 'Frame rot check on every install'
                },
                {
                    feature: 'Old Door Removal',
                    others: 'Extra charge',
                    homeyy: 'Included in every installation'
                },

                {
                    feature: 'Security Upgrade',
                    others: 'Door only',
                    homeyy: 'Multi-point lock upgrade available'
                },
                {
                    feature: 'Cleanup',
                    others: 'Debris left for homeowner',
                    homeyy: 'Full haul-away and cleanup'
                },
            ]
        },

        flooring: {
            tag: 'WHY HOMEYY',
            title: "Every Flooring Company Promises Quality. Here's How We Back It Up.",

            features: [
                {
                    feature: 'Quote Turnaround',
                    others: '3–5 days',
                    homeyy: 'Same day'
                },
                {
                    feature: 'Sample Process',
                    others: 'Visit showroom yourself',
                    homeyy: 'We bring samples to your home'
                },
                {
                    feature: 'Pricing',
                    others: 'Per sq ft only — extras hidden',
                    homeyy: 'Full project fixed price quoted'
                },
                {
                    feature: 'Subfloor Inspection',
                    others: 'Not included',
                    homeyy: 'Included before every install'
                },
                {
                    feature: 'Furniture Moving',
                    others: 'Extra charge',
                    homeyy: 'Included — we move and replace'
                },
                {
                    feature: 'Old Floor Removal',
                    others: 'Charged separately',
                    homeyy: 'Included in project price'
                },
                {
                    feature: 'Post-Install Cleanup',
                    others: 'Debris left on-site',
                    homeyy: 'Full cleanup — floors ready to use'
                },
                // {
                //     feature: 'Financing',
                //     others: 'Not available',
                //     homeyy: '0% financing available'
                // }
            ]
        },

        gutter: {
            tag: 'WHY HOMEYY',
            title: "Good Gutters or Great Gutters — Here's the Difference",

            features: [
                {
                    feature: 'Response Time',
                    others: '1–2 weeks out',
                    homeyy: 'Same week available'
                },
                {
                    feature: 'Quote Turnaround',
                    others: '2–3 days',
                    homeyy: 'Same day'
                },
                {
                    feature: 'Gutter Type',
                    others: 'Sectional (leaks at seams)',
                    homeyy: 'Seamless — custom cut on-site'
                },
                {
                    feature: 'Gutter Guards',
                    others: 'Sold separately, not installed',
                    homeyy: 'Supply and install available'
                },
                {
                    feature: 'Downspout Check',
                    others: 'Not included',
                    homeyy: 'Checked and cleared every visit'
                },

                {
                    feature: 'Debris Disposal',
                    others: 'Left for homeowner',
                    homeyy: 'Full debris removal included'
                },
                // {
                //     feature: 'Financing',
                //     others: 'Not available',
                //     homeyy: '0% financing on full gutter systems'
                // }
            ]
        },

        fencing: {
            tag: 'WHWHY HOMEYY',
            title: "Why Homeyy Fences Outlast the Competition ",

            features: [
                {
                    feature: 'Quote Turnaround',
                    others: '3–5 days',
                    homeyy: 'Same day'
                },
                {
                    feature: 'Permit Management',
                    others: "Homeowner's responsibility",
                    homeyy: 'We pull and manage all permits'
                },
                {
                    feature: 'Property Line Check',
                    others: 'Not offered',
                    homeyy: 'Review before every install'
                },
                {
                    feature: 'Pricing',
                    others: 'Changes after material order',
                    homeyy: 'Fixed price locked before start'
                },
                // {
                //     feature: 'Post Setting',
                //     others: 'Manual — may shift over time',
                //     homeyy: 'Concrete-set — guaranteed plumb'
                // },
                {
                    feature: 'Gate Hardware',
                    others: 'Basic latch included',
                    homeyy: 'Heavy-duty hardware, self-closing'
                },
                {
                    feature: 'HOA Approval Help',
                    others: 'Not offered',
                    homeyy: 'We prepare HOA documentation'
                },
                {
                    feature: 'Old Fence Removal',
                    others: 'Extra charge',
                    homeyy: 'Included in project price'
                },

                // {
                //     feature: 'Financing',
                //     others: 'Not available',
                //     homeyy: '0% financing on full installs'
                // }
            ]
        },

        solar: {
            tag: 'WHY HOMEYY',
            title: "The Solar Company That Works for You, Not the Grid",

            features: [
                {
                    feature: 'Assessment',
                    others: 'Generic online calculator',
                    homeyy: 'Free in-home energy assessment'
                },
                // {
                //     feature: 'Financing',
                //     others: 'Loans with interest',
                //     homeyy: '$0 down, 0% financing available'
                // },
                {
                    feature: 'Tax Credit Guidance',
                    others: 'Not provided',
                    homeyy: 'We calculate your exact credit'
                },
                {
                    feature: 'Permitting',
                    others: 'Homeowner manages',
                    homeyy: 'We handle all permits & utility approvals'
                },
                {
                    feature: 'Equipment Brand',
                    others: 'Off-brand panels',
                    homeyy: 'Tier 1 panels — top-5 manufacturers'
                },
                {
                    feature: 'Installation Timeline',
                    others: '8–16 weeks',
                    homeyy: '3–5 weeks start to finish'
                },
                {
                    feature: 'Monitoring',
                    others: 'Basic app only',
                    homeyy: '24/7 performance monitoring included'
                },

                {
                    feature: 'Battery Storage',
                    others: 'Not offered',
                    homeyy: 'Powerwall and storage options'
                }
            ]
        },

        homesecurity: {
            tag: 'WHY HOMEYY',
            title: "Why Homeyy — Not the Other Guys",

            features: [
                {
                    feature: 'Contract Required',
                    others: '24–36 month lock-in',
                    homeyy: 'No long-term contract'
                },
                {
                    feature: 'Equipment Ownership',
                    others: 'Company-owned — left if cancel',
                    homeyy: 'You own the equipment outright'
                },
                {
                    feature: 'Installation',
                    others: 'DIY self-install',
                    homeyy: 'Professional installation'
                },
                {
                    feature: 'Monitoring Options',
                    others: 'Mandatory paid plan',
                    homeyy: 'Optional — your choice'
                },
                {
                    feature: 'Smart Home Integration',
                    others: 'Limited, proprietary only',
                    homeyy: 'Works with Alexa, Google, Apple'
                },
                {
                    feature: 'Camera Quality',
                    others: '720p basic cameras',
                    homeyy: '4K with night vision & AI detection'
                },
                // {
                //     feature: 'Response Time',
                //     others: 'Alert sent to phone only',
                //     homeyy: '24/7 live monitoring centre'
                // },
                {
                    feature: 'System Expansion',
                    others: 'Locked to their ecosystem',
                    homeyy: 'Open system — add any device'
                },
                {
                    feature: 'Relocation',
                    others: 'System left behind',
                    homeyy: 'System moves with you'
                },
                // {
                //     feature: 'Financing',
                //     others: 'Monthly rental only',
                //     homeyy: '0% purchase financing available'
                // }
            ]
        },

        kitchen: {
            tag: 'WHY HOMEYY',
            title: "Siding That Fits Your Home, Your Climate, and Your Budget",

            features: [
                {
                    feature: 'Quote Turnaround',
                    others: '5–7 days',
                    homeyy: 'Same day'
                },
                {
                    feature: 'Design Consultation',
                    others: 'Paid add-on',
                    homeyy: 'Free in-home consultation'
                },
                {
                    feature: '3D Rendering',
                    others: 'Not provided',
                    homeyy: 'Included before you commit'
                },
                {
                    feature: 'Pricing',
                    others: 'Quote changes mid-project',
                    homeyy: 'Fixed price — locked in writing'
                },
                {
                    feature: 'Cabinet Lead Time',
                    others: 'Surprise delays after you sign',
                    homeyy: 'Confirmed before project starts'
                },
                {
                    feature: 'Crew',
                    others: 'Multiple subcontractors',
                    homeyy: 'Single in-house team'
                },
                {
                    feature: 'Daily Cleanup',
                    others: 'End of project only',
                    homeyy: 'Every day without exception'
                },

                // {
                //     feature: 'Financing',
                //     others: 'Not available',
                //     homeyy: '0% financing on full remodels'
                // }
            ]
        },


    };

    // *\----------------------------- testimonial Content  ---------------------------\*

    testimonialContent: any = {

        roofing: {
            tag: 'TESTIMONIALS',
            titleLine1: 'What Our Clients',
            titleLine2: 'Are Saying?',
            description:
                'Hear from our satisfied clients who trust us for top-quality service,reliable results, and exceptional customer care on every project.',

            reviews: [
                {
                    name: 'Sarah M',
                    location: 'Madrid, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "They were on my roof in 3 hours after my call. The crew was professional, the cleanup was spotless, and the whole replacement was done before dinner. I've never had a contractor experience like this."
                },
                {
                    name: 'James T',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "After the hailstorm I was overwhelmed. Contractors handled everything — the inspection, photos. I didn't have to do anything except approve the work. Roof looks incredible"
                },
                {
                    name: 'Sarah M',
                    location: 'Madrid, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "They were on my roof in 3 hours after my call. The crew was professional, the cleanup was spotless, and the whole replacement was done before dinner. I've never had a contractor experience like this."
                },
                {
                    name: 'James T',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "After the hailstorm I was overwhelmed. Contractors handled everything — the inspection, photos. I didn't have to do anything except approve the work. Roof looks incredible"
                },
            ]
        },

        bathroom: {
            tag: 'TESTIMONIALS',
            titleLine1: 'What Our Clients',
            titleLine2: 'Are Saying?',
            description:
                'Hear from our satisfied clients who trust us for top-quality service, reliable results, and exceptional customer care on every project.',

            reviews: [
                {
                    name: 'SaraRachel & Tom H',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "We were nervous after a bad experience with another contractor. Contractors walked us through every step, showed us samples, and finished 2 days ahead of schedule. The tile work is absolutely stunning."
                },
                {
                    name: 'David M',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "I expected bathroom remodels to be stressful and over-budget. Contractors gave me a fixed price, stuck to it, and transformed our outdated bathroom into something we're genuinely proud to show guests."
                },
                {
                    name: 'SaraRachel & Tom H',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "We were nervous after a bad experience with another contractor. Contractors walked us through every step, showed us samples, and finished 2 days ahead of schedule. The tile work is absolutely stunning."
                },
                {
                    name: 'David M',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "I expected bathroom remodels to be stressful and over-budget. Contractors gave me a fixed price, stuck to it, and transformed our outdated bathroom into something we're genuinely proud to show guests."
                }
            ]
        },

        window: {
            tag: 'TESTIMONIALS',
            titleLine1: 'What Our Clients',
            titleLine2: 'Are Saying?',
            description:
                'Hear from our satisfied clients who trust us for top-quality service,reliable results, and exceptional customer care on every project.',

            reviews: [
                {
                    name: 'Karen S',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "We replaced 14 windows and our first heating bill dropped $112. The installers were meticulous — they even patched the trim perfectly. I wished I'd done this 5 years ago."
                },
                {
                    name: 'Phil T',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        'Hear from our satisfied clients who trust us for top-quality service, reliable results, and exceptional customer care on every project.',
                },
                {
                    name: 'Karen S',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "We replaced 14 windows and our first heating bill dropped $112. The installers were meticulous — they even patched the trim perfectly. I wished I'd done this 5 years ago."
                },
                {
                    name: 'Phil T',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        'Hear from our satisfied clients who trust us for top-quality service, reliable results, and exceptional customer care on every project.'
                },
            ]
        },

        hvac: {
            tag: 'TESTIMONIALS',
            titleLine1: 'What Our Clients',
            titleLine2: 'Are Saying?',
            description:
                'Hear from our satisfied clients who trust us for top-quality service,reliable results, and exceptional customer care on every project.',

            reviews: [
                {
                    name: 'Sarah M',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "Our AC died at 9pm on a Friday during a heat wave. I called Contractors and a tech was at our door by 10:30. Fixed it in 45 minutes — cost exactly what they quoted. Unbelievable service."
                },
                {
                    name: 'Linda K',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "I got 3 quotes for a new HVAC system. Contractors was the only one who showed me a repair-vs-replace breakdown and didn't pressure me. Went with them. Install was clean, fast, and the energy bill dropped $80/month."
                },
                {
                    name: 'Sarah M',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "Our AC died at 9pm on a Friday during a heat wave. I called Contractors and a tech was at our door by 10:30. Fixed it in 45 minutes — cost exactly what they quoted. Unbelievable service."
                },
                {
                    name: 'Linda K',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "I got 3 quotes for a new HVAC system. Contractors was the only one who showed me a repair-vs-replace breakdown and didn't pressure me. Went with them. Install was clean, fast, and the energy bill dropped $80/month."
                },
            ]
        },

        siding: {
            tag: 'TESTIMONIALS',
            titleLine1: 'What Our Clients',
            titleLine2: 'Are Saying?',
            description:
                'Hear from our satisfied clients who trust us for top-quality service,reliable results, and exceptional customer care on every project.',

            reviews: [
                {
                    name: 'Jim & Carol B',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "After the hailstorm, Contractors came out within 48 hours, documented all the damage with photos. New fiber cement siding installed 3 weeks later — looks like a completely different house."
                },
                {
                    name: 'Amanda P',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "We'd been putting off residing for 3 years because we were overwhelmed by the options. Contractor's consultant spent an hour with us, brought samples, and helped us pick something that looks amazing. Neighbors keep stopping to ask who we used."
                },
                {
                    name: 'Jim & Carol B',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "After the hailstorm, Contractors came out within 48 hours, documented all the damage with photos.. New fiber cement siding installed 3 weeks later — looks like a completely different house."
                },
                {
                    name: 'Amanda P',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "We'd been putting off residing for 3 years because we were overwhelmed by the options. Contractor's consultant spent an hour with us, brought samples, and helped us pick something that looks amazing. Neighbors keep stopping to ask who we used."
                }
            ]
        },

        plumbing: {
            tag: 'TESTIMONIALS',
            titleLine1: 'What Our Clients',
            titleLine2: 'Are Saying?',
            description:
                'Hear from our satisfied clients who trust us for top-quality service,reliable results, and exceptional customer care on every project.',

            reviews: [
                {
                    name: 'Derek M',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "Pipe burst at 11pm on a Sunday. Contractors had a plumber at my door by midnight and the pipe fixed by 1am. Price was exactly what they quoted over the phone. Absolutely incredible service."
                },
                {
                    name: 'Sandra P',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "Tried two other plumbers who couldn't find our slab leak. Contractors used a camera and found it in 20 minutes. Fixed, tested, and cleaned up the same afternoon. Should have called them first."
                },
                {
                    name: 'Derek M',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "Pipe burst at 11pm on a Sunday. Contractors had a plumber at my door by midnight and the pipe fixed by 1am. Price was exactly what they quoted over the phone. Absolutely incredible service."
                },
                {
                    name: 'Sandra P',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "Tried two other plumbers who couldn't find our slab leak. Contractors used a camera and found it in 20 minutes. Fixed, tested, and cleaned up the same afternoon. Should have called them first."
                }
            ]
        },

        door: {
            tag: 'TESTIMONIALS',
            titleLine1: 'What Our Clients',
            titleLine2: 'Are Saying?',
            description:
                'Hear from our satisfied clients who trust us for top-quality service,reliable results, and exceptional customer care on every project.',

            reviews: [
                {
                    name: 'Helen R',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "Our front door was warped and wouldn't close properly for two winters. Homeyy came, spotted a rotted frame we didn't know about, fixed it, and installed a beautiful new door — all in one day. Fixed price, no surprises."
                },
                {
                    name: 'Marcus T',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "Needed a full patio door replacement after storm damage. Homeyy had the door measured, ordered, and installed within a week. The new sliding door is smoother than anything we've had before and the weathersealing is perfect."
                },
                {
                    name: 'Helen R',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "Our front door was warped and wouldn't close properly for two winters. Homeyy came, spotted a rotted frame we didn't know about, fixed it, and installed a beautiful new door — all in one day. Fixed price, no surprises."
                },
                {
                    name: 'Marcus T',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "Needed a full patio door replacement after storm damage. Homeyy had the door measured, ordered, and installed within a week. The new sliding door is smoother than anything we've had before and the weathersealing is perfect."
                }
            ]
        },

        flooring: {
            tag: 'TESTIMONIALS',
            titleLine1: 'What Our Clients',
            titleLine2: 'Are Saying?',
            description:
                'Hear from our satisfied clients who trust us for top-quality service,reliable results, and exceptional customer care on every project.',

            reviews: [
                {
                    name: 'Joanna & Pete L',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "We had carpet in every room and always wanted hardwood. Contractors came out with samples, gave us a fixed quote the same day, and installed everything in 3 days. The house is transformed. Every visitor comments on the floors."
                },
                {
                    name: 'Brian S',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "I was quoted $18,000 by another company for full-home LVP. Contractors quoted $11,500 — fixed price, includes old floor removal and furniture moving. Same quality, done in 4 days. I can't recommend them enough."
                },
                {
                    name: 'Joanna & Pete L',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "We had carpet in every room and always wanted hardwood. Contractors came out with samples, gave us a fixed quote the same day, and installed everything in 3 days. The house is transformed. Every visitor comments on the floors."
                },
                {
                    name: 'Brian S',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "I was quoted $18,000 by another company for full-home LVP. Contractors quoted $11,500 — fixed price, includes old floor removal and furniture moving. Same quality, done in 4 days. I can't recommend them enough."
                }
            ]
        },

        gutter: {
            tag: 'TESTIMONIALS',
            titleLine1: 'What Our Clients',
            titleLine2: 'Are Saying?',
            description:
                'Hear from our satisfied clients who trust us for top-quality service,reliable results, and exceptional customer care on every project.',

            reviews: [
                {
                    name: 'Tom & Louise B',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "Our gutters were overflowing every rain and pulling away from the fascia. Contractors came the same week, showed us the rotted fascia we didn't know about, replaced it, and installed new seamless gutters with guards — all in one day. No more ladder trips."
                },
                {
                    name: 'Andrea F',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "The gutter guards they installed have been through two full leaf seasons and I haven't touched a ladder once. Worth every cent. The team was fast, left the yard completely clean, and the colour match to our trim is perfect."
                },
                {
                    name: 'Tom & Louise B',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "Our gutters were overflowing every rain and pulling away from the fascia. Contractors came the same week, showed us the rotted fascia we didn't know about, replaced it, and installed new seamless gutters with guards — all in one day. No more ladder trips."
                },
                {
                    name: 'Andrea F',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "The gutter guards they installed have been through two full leaf seasons and I haven't touched a ladder once. Worth every cent. The team was fast, left the yard completely clean, and the colour match to our trim is perfect."
                }
            ]
        },

        fencing: {
            tag: 'TESTIMONIALS',
            titleLine1: 'What Our Clients',
            titleLine2: 'Are Saying?',
            description:
                'Hear from our satisfied clients who trust us for top-quality service,reliable results, and exceptional customer care on every project.',

            reviews: [
                {
                    name: 'Chris & Dana M',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "Contractors gave us a same-day quote, pulled the permit, and had our full cedar privacy fence installed in 2 days. The concrete-set posts are perfectly plumb and the finish is beautiful."
                },
                {
                    name: 'Kelly W',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "Our old fence was a hazard after a windstorm. Contractors was out the next morning, removed the old fence, and installed new vinyl panels by end of day. Price was exactly as quoted. They even helped us with the HOA colour approval."
                },
                {
                    name: 'JiChris & Dana M',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "Contractors gave us a same-day quote, pulled the permit, and had our full cedar privacy fence installed in 2 days. The concrete-set posts are perfectly plumb and the finish is beautiful."
                },
                {
                    name: 'Kelly W',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "Our old fence was a hazard after a windstorm. Contractors was out the next morning, removed the old fence, and installed new vinyl panels by end of day. Price was exactly as quoted. They even helped us with the HOA colour approval."
                }
            ]
        },

        solar: {
            tag: 'TESTIMONIALS',
            titleLine1: 'What Our Clients',
            titleLine2: 'Are Saying?',
            description:
                'Hear from our satisfied clients who trust us for top-quality service,reliable results, and exceptional customer care on every project.',

            reviews: [
                {
                    name: 'Gary & Kim T',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "We were sceptical that solar would work for our home. Contractors came out, showed us exactly how much we'd produce and save, and the numbers were better than we expected. System was installed in 3 weeks, and our last three bills have been under $20."
                },
                {
                    name: 'Michelle R',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "The $0 down option meant we started saving from day one — no upfront cost and the monthly savings exceed the finance payment. Homeyy handled permits, the utility inspection, everything. We did nothing except sign and watch the savings pile up."
                },
                {
                    name: 'Gary & Kim T',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "We were sceptical that solar would work for our home. Contractors came out, showed us exactly how much we'd produce and save, and the numbers were better than we expected. System was installed in 3 weeks, and our last three bills have been under $20."
                },
                {
                    name: 'Michelle R',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "The $0 down option meant we started saving from day one — no upfront cost and the monthly savings exceed the finance payment. Homeyy handled permits, the utility inspection, everything. We did nothing except sign and watch the savings pile up."
                }
            ]
        },

        homesecurity: {
            tag: 'TESTIMONIALS',
            titleLine1: 'What Our Clients',
            titleLine2: 'Are Saying?',
            description:
                'Hear from our satisfied clients who trust us for top-quality service,reliable results, and exceptional customer care on every project.',

            reviews: [
                {
                    name: 'James L',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "I'd been burned by a big security company's 3-year contract before. Contractors installed a full system — cameras, sensors, smart locks — in one day, I own everything, and I choose month-to-month monitoring. Exactly what I wanted."
                },
                {
                    name: 'Priya M',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "After a break-in two streets over, I called Contractors the same day. They did a full assessment, pointed out vulnerabilities I hadn't thought of, and had cameras and a full alarm installed by the following afternoon. I sleep so much better now."
                },
                {
                    name: 'James L',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "I'd been burned by a big security company's 3-year contract before. Contractors installed a full system — cameras, sensors, smart locks — in one day, I own everything, and I choose month-to-month monitoring. Exactly what I wanted."
                },
                {
                    name: 'Priya M',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "After a break-in two streets over, I called Contractors the same day. They did a full assessment, pointed out vulnerabilities I hadn't thought of, and had cameras and a full alarm installed by the following afternoon. I sleep so much better now."
                }
            ]
        },

        kitchen: {
            tag: 'TESTIMONIALS',
            titleLine1: 'What Our Clients',
            titleLine2: 'Are Saying?',
            description:
                'Hear from our satisfied clients who trust us for top-quality service,reliable results, and exceptional customer care on every project.',

            reviews: [
                {
                    name: 'Karen & Joel M',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "We were quoted 10 weeks by two other contractors. Contractors came in with a 3D rendering on the first visit, gave us a fixed price, and finished in 7 weeks. The kitchen looks like something out of a magazine."
                },
                {
                    name: 'Thomas R',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "I was terrified of going over budget — it’s the number one thing everyone warns you about with kitchen remodels. [Company] gave me a locked price on day one and honoured it completely. Not a dollar more. I still can’t believe it."
                },
                {
                    name: 'Karen & Joel M',
                    location: 'Mike D, Spain',
                    image: '../../../../assets/images/home/client_1.png',
                    review:
                        "We were quoted 10 weeks by two other contractors. Contractors came in with a 3D rendering on the first visit, gave us a fixed price, and finished in 7 weeks. The kitchen looks like something out of a magazine."
                },
                {
                    name: 'Thomas R',
                    location: 'New York, USA',
                    image: '../../../../assets/images/home/client_2.png',
                    review:
                        "I was terrified of going over budget — it’s the number one thing everyone warns you about with kitchen remodels. [Company] gave me a locked price on day one and honoured it completely. Not a dollar more. I still can’t believe it."
                }
            ]
        },

    };

}

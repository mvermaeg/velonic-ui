import { Component } from '@angular/core';

@Component({
  selector: 'app-privacy-policy',
  templateUrl: './privacy-policy.component.html',
  styleUrls: ['./privacy-policy.component.scss'],
})
export class PrivacyPolicyComponent {

  LAST_MODIFIED = 'June 4, 2025';

  // Categories of Personal Information collected — CCPA disclosure table
  ccpaCategories = [
    {
      code: 'A',
      category: 'Identifiers',
      examples: `A real name, alias, postal address, unique personal identifier, online identifier, Internet Protocol
        address, email address, account name, Social Security number, driver's license number, passport number, or other
        similar identifiers.`,
      collected: true
    },
    {
      code: 'B',
      category: `Personal information categories listed in the California Customer Records statute
        (Cal. Civ. Code § 1798.80(e))`,
      examples: `A name, signature, Social Security number, physical characteristics or description, address, telephone
        number, passport number, driver's license or state identification card number, insurance policy number,
        education, employment history, bank account number, credit card number, debit card number, or any other
        financial information, medical information, or health insurance information.`,
      collected: true
    },
    {
      code: 'C',
      category: 'Protected classification characteristics under California or federal law',
      examples: `Age (40 years or older), race, color, ancestry, national origin, citizenship, religion or creed,
        marital status, medical condition, physical or mental disability, sex (including gender, gender identity, gender
        expression, pregnancy or childbirth and related medical conditions), sexual orientation, veteran or military
        status, genetic information (including familial genetic information).`,
      collected: false
    },
    {
      code: 'D',
      category: 'Commercial information',
      examples: `Records of personal property, products or services purchased, obtained, or considered, or other
        purchasing or consuming histories or tendencies.`,
      collected: false
    },
    {
      code: 'E',
      category: 'Biometric information',
      examples: `Genetic, physiological, behavioral, and biological characteristics, or activity patterns used to
        extract a template or other identifier or identifying information, such as fingerprints, faceprints, and
        voiceprints, iris or retina scans, keystroke, gait, or other physical patterns, and sleep, health, or exercise
        data.`,
      collected: false
    },
    {
      code: 'F',
      category: 'Internet or other similar network activity',
      examples: `Browsing history, search history, information on a consumer's interaction with a website, application,
        or advertisement.`,
      collected: true
    },
    {
      code: 'G',
      category: 'Geolocation data',
      examples: 'Physical location or movements.',
      collected: true
    },
    {
      code: 'H',
      category: 'Sensory data',
      examples: 'Audio, electronic, visual, thermal, olfactory, or similar information.',
      collected: false
    },
    {
      code: 'I',
      category: 'Professional or employment-related information',
      examples: 'Current or past job history or performance evaluations.',
      collected: false
    },
    {
      code: 'J',
      category: `Non-public education information (per the Family Educational Rights and Privacy Act
        (20 U.S.C. Section 1232g, 34 C.F.R. Part 99))`,
      examples: `Education records directly related to a student maintained by an educational institution or party
        acting on its behalf, such as grades, transcripts, class lists, student schedules, student identification codes,
        student financial information, or student disciplinary records.`,
      collected: false
    },
    {
      code: 'K',
      category: 'Inferences drawn from other personal information',
      examples: `Profile reflecting a person's preferences, characteristics, psychological trends, predispositions,
        behavior, attitudes, intelligence, abilities, and aptitudes.`,
      collected: true
    }
  ];

  // Categories of Personal Information disclosed for a business purpose
  disclosedCategories = [
    'Category A. Identifiers',
    'Category B. California Consumer personal information',
    'Category E. California Consumer commercial information',
    'Category F. California Consumer internet or other similar network activity',
    'Category G. California Consumer geolocation data',
    'Category K. Inferences drawn from other personal information'
  ];

  // Categories of Personal Information sold or shared with Partners and/or Contractors
  sharedCategories = [
    'Category A. Identifiers',
    'Category D. California Consumer personal information',
    'Category E. California Consumer commercial information',
    'Category F. California Consumer internet or other similar network activity',
    'Category G. California Consumer geolocation data',
    'Category K. Inferences drawn from other personal information'
  ];

}

import fs from 'node:fs';
import path from 'node:path';

const root = process.cwd();
const productDir = path.join(root, 'backend', 'Noor.Api', 'wwwroot', 'images', 'products');
const categoryDir = path.join(root, 'backend', 'Noor.Api', 'wwwroot', 'images', 'categories');
const editorialDir = path.join(root, 'frontend', 'noor-web', 'public', 'assets');
for (const dir of [productDir, categoryDir, editorialDir]) fs.mkdirSync(dir, { recursive: true });

const palettes = [
  { bg1: '#f2eadc', bg2: '#ded1bd', ink: '#242019', fabric1: '#2e2a24', fabric2: '#51483d', accent: '#a8834b', soft: '#cbbda6' },
  { bg1: '#eee4d4', bg2: '#d8c6ad', ink: '#3d2f25', fabric1: '#6f5847', fabric2: '#957a62', accent: '#a56f48', soft: '#cbb69d' },
  { bg1: '#e9e7db', bg2: '#d2d0c1', ink: '#303327', fabric1: '#59604d', fabric2: '#7c836c', accent: '#9a8a54', soft: '#c3c1af' },
  { bg1: '#eee3df', bg2: '#dccbc6', ink: '#3b2830', fabric1: '#593646', fabric2: '#7a5262', accent: '#a87c58', soft: '#d1bab4' },
  { bg1: '#e8e9e6', bg2: '#d3d6d2', ink: '#2d3337', fabric1: '#3d484f', fabric2: '#5c6870', accent: '#9e947f', soft: '#c3c9c5' },
  { bg1: '#f1e8e0', bg2: '#e2d2c5', ink: '#493229', fabric1: '#7c574b', fabric2: '#9c7668', accent: '#ab8452', soft: '#d6c4b4' }
];

const products = [
  ['Layla Everyday Burqa', 'burqas', 'BURQA', 0],
  ['Amara Pleated Burqa', 'burqas', 'BURQA', 1],
  ['Noor Signature Embroidered Burqa', 'burqas', 'SIGNATURE', 3],
  ['Safiya Two-Piece Burqa', 'burqas', 'BURQA', 2],
  ['Hana Travel Burqa', 'burqas', 'BURQA', 4],
  ['Maryam Occasion Burqa', 'burqas', 'OCCASION', 3],
  ['Zahra Flowing Abaya', 'abayas', 'ABAYA', 0],
  ['Inaya Linen Abaya', 'abayas', 'ABAYA', 5],
  ['Soraya Belted Abaya', 'abayas', 'ABAYA', 2],
  ['Dalia Kimono Abaya', 'abayas', 'ABAYA', 1],
  ['Classic Modal Hijab', 'hijabs', 'HIJAB', 5],
  ['Crinkled Chiffon Hijab', 'hijabs', 'HIJAB', 4],
  ['Silk Blend Hijab', 'hijabs', 'HIJAB', 3],
  ['Jersey Everyday Hijab', 'hijabs', 'HIJAB', 0],
  ['Soft Touch Niqab', 'niqabs', 'NIQAB', 0],
  ['Tie-Back Niqab', 'niqabs', 'NIQAB', 1],
  ['One-Piece Prayer Dress', 'prayer-wear', 'PRAYER WEAR', 2],
  ['Travel Prayer Mat', 'prayer-wear', 'PRAYER WEAR', 5],
  ['Magnetic Hijab Pins', 'accessories', 'ACCESSORIES', 4],
  ['Under-Cap Set', 'accessories', 'ACCESSORIES', 1]
];

const slugify = (value) => value.toLowerCase().replace(/&/g, 'and').replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '');

const defs = (p, id) => `
  <defs>
    <linearGradient id="bg${id}" x1="0" y1="0" x2="1" y2="1">
      <stop offset="0" stop-color="${p.bg1}"/><stop offset="1" stop-color="${p.bg2}"/>
    </linearGradient>
    <linearGradient id="fabric${id}" x1="0.1" y1="0" x2="0.95" y2="1">
      <stop offset="0" stop-color="${p.fabric2}"/><stop offset="0.45" stop-color="${p.fabric1}"/><stop offset="1" stop-color="${p.ink}"/>
    </linearGradient>
    <linearGradient id="veil${id}" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="${p.fabric2}" stop-opacity="0.72"/><stop offset="1" stop-color="${p.fabric1}" stop-opacity="0.96"/>
    </linearGradient>
    <filter id="grain${id}"><feTurbulence type="fractalNoise" baseFrequency="0.85" numOctaves="2" stitchTiles="stitch"/><feColorMatrix type="saturate" values="0"/></filter>
  </defs>`;

const figure = (p, id, kind) => {
  if (kind === 'hijabs') {
    return `
      <g opacity="0.96">
        <path d="M188 830 C 150 650 208 408 400 344 C 594 408 650 650 612 830 C 554 902 246 902 188 830 Z" fill="url(#veil${id})"/>
        <path d="M248 760 C 222 610 274 454 400 414 C 526 454 578 610 552 760 C 514 808 286 808 248 760 Z" fill="${p.ink}" opacity="0.22"/>
        <path d="M258 560 C 320 516 480 516 542 560 C 508 622 292 622 258 560 Z" fill="${p.bg1}" opacity="0.20"/>
        <path d="M206 740 C 246 646 300 596 400 570 C 500 596 554 646 594 740" fill="none" stroke="${p.soft}" stroke-opacity="0.45" stroke-width="2"/>
        <path d="M182 852 C 286 906 514 906 618 852" fill="none" stroke="${p.accent}" stroke-opacity="0.55" stroke-width="2"/>
      </g>`;
  }
  if (kind === 'accessories') {
    return `
      <g opacity="0.95">
        <circle cx="400" cy="565" r="176" fill="${p.bg1}" opacity="0.35" stroke="${p.soft}" stroke-width="2"/>
        <circle cx="400" cy="565" r="132" fill="none" stroke="${p.accent}" stroke-opacity="0.6" stroke-width="1.5"/>
        <circle cx="352" cy="528" r="46" fill="url(#fabric${id})"/>
        <circle cx="448" cy="528" r="46" fill="url(#fabric${id})"/>
        <path d="M326 660 Q 400 710 474 660" fill="none" stroke="${p.accent}" stroke-width="3" stroke-linecap="round"/>
        <circle cx="400" cy="565" r="7" fill="${p.accent}"/>
      </g>`;
  }
  if (kind === 'prayer-wear') {
    return `
      <g opacity="0.96">
        <path d="M210 840 C 182 650 238 466 400 424 C 562 466 618 650 590 840 C 520 880 280 880 210 840 Z" fill="url(#fabric${id})"/>
        <path d="M400 424 C 336 446 302 500 304 552 C 350 582 450 582 496 552 C 498 500 464 446 400 424 Z" fill="${p.fabric2}" opacity="0.7"/>
        <path d="M400 424 C 372 320 282 296 238 344 C 286 358 338 398 354 448" fill="none" stroke="${p.fabric2}" stroke-opacity="0.8" stroke-width="28" stroke-linecap="round"/>
        <path d="M258 698 C 336 726 464 726 542 698" fill="none" stroke="${p.soft}" stroke-opacity="0.28" stroke-width="2"/>
        <path d="M286 780 C 344 798 456 798 514 780" fill="none" stroke="${p.soft}" stroke-opacity="0.2" stroke-width="2"/>
      </g>`;
  }

  const hood = kind === 'niqabs'
    ? `<path d="M400 244 C 326 252 292 316 294 384 L 306 474 L 494 474 L 506 384 C 508 316 474 252 400 244 Z" fill="url(#fabric${id})"/>
       <path d="M308 386 C 362 404 438 404 492 386 L 486 434 C 438 454 362 454 314 434 Z" fill="${p.ink}" opacity="0.9"/>`
    : `<path d="M400 240 C 330 248 294 310 296 378 L 304 456 L 496 456 L 504 378 C 506 310 470 248 400 240 Z" fill="url(#fabric${id})"/>
       <path d="M400 272 C 352 278 328 322 330 370 C 362 388 438 388 470 370 C 472 322 448 278 400 272 Z" fill="${p.ink}" opacity="0.28"/>`;

  return `
    <g opacity="0.97">
      <path d="M400 240 C 330 248 294 310 296 378 L 304 456 C 280 522 262 620 258 716 L 244 836 L 556 836 L 542 716 C 538 620 520 522 496 456 L 504 378 C 506 310 470 248 400 240 Z" fill="url(#fabric${id})"/>
      ${hood}
      <path d="M300 500 C 260 592 248 704 246 810" fill="none" stroke="${p.soft}" stroke-opacity="0.18" stroke-width="2"/>
      <path d="M500 500 C 540 592 552 704 554 810" fill="none" stroke="${p.soft}" stroke-opacity="0.14" stroke-width="2"/>
      <path d="M334 636 C 368 654 432 654 466 636" fill="none" stroke="${p.soft}" stroke-opacity="0.16" stroke-width="2"/>
      <path d="M318 744 C 368 764 432 764 482 744" fill="none" stroke="${p.soft}" stroke-opacity="0.12" stroke-width="2"/>
    </g>`;
};

const frame = (p, id) => `
  <circle cx="652" cy="196" r="118" fill="${p.accent}" opacity="0.13"/>
  <circle cx="652" cy="196" r="88" fill="none" stroke="${p.accent}" stroke-opacity="0.22" stroke-width="1"/>
  <path d="M160 858 L160 420 A 240 240 0 0 1 640 420 L640 858" fill="none" stroke="${p.soft}" stroke-opacity="0.75" stroke-width="1.2"/>
  <path d="M140 858 L140 412 A 260 260 0 0 1 660 412 L660 858" fill="none" stroke="${p.accent}" stroke-opacity="0.15" stroke-width="1"/>`;

const signature = (p, name, kicker, number, id) => `
  <g fill="${p.ink}">
    <text x="58" y="78" font-family="Georgia, 'Times New Roman', serif" font-size="17" letter-spacing="7">NOOR</text>
    <text x="58" y="101" font-family="Arial, sans-serif" font-size="8" letter-spacing="3" opacity="0.58">MODEST ATELIER</text>
    <text x="58" y="918" font-family="Arial, sans-serif" font-size="9" letter-spacing="4" opacity="0.6">${kicker}</text>
    <text x="58" y="955" font-family="Georgia, 'Times New Roman', serif" font-size="29">${name}</text>
    <text x="742" y="955" text-anchor="end" font-family="Georgia, 'Times New Roman', serif" font-size="15" opacity="0.7">N° ${number}</text>
    <line x1="58" y1="882" x2="742" y2="882" stroke="${p.ink}" stroke-opacity="0.18" stroke-width="1"/>
  </g>`;

const productSvg = (name, kind, p, variant) => {
  const id = variant;
  const safeName = name.length > 40 ? name.slice(0, 38) + '…' : name;
  if (variant === 2) {
    return `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 1000" width="800" height="1000">
  ${defs(p, 2)}
  <rect width="800" height="1000" fill="url(#bg2)"/>
  <rect x="54" y="54" width="692" height="892" fill="none" stroke="${p.ink}" stroke-opacity="0.18"/>
  <g opacity="0.94">
    <path d="M94 730 C 236 600 322 392 400 292 C 478 392 564 600 706 730 L 706 910 L 94 910 Z" fill="url(#fabric2)"/>
    <path d="M142 760 C 270 646 338 492 400 412 C 462 492 530 646 658 760" fill="none" stroke="${p.soft}" stroke-opacity="0.3" stroke-width="2"/>
    <path d="M180 790 C 296 690 352 558 400 492 C 448 558 504 690 620 790" fill="none" stroke="${p.soft}" stroke-opacity="0.18" stroke-width="2"/>
    <path d="M94 838 C 260 786 540 786 706 838" fill="none" stroke="${p.accent}" stroke-opacity="0.4" stroke-width="1.5"/>
    <circle cx="650" cy="196" r="74" fill="none" stroke="${p.accent}" stroke-opacity="0.35"/>
    <circle cx="650" cy="196" r="56" fill="${p.accent}" opacity="0.13"/>
  </g>
  <rect width="800" height="1000" filter="url(#grain2)" opacity="0.05"/>
  <g fill="${p.bg1}">
    <text x="88" y="96" font-family="Georgia, 'Times New Roman', serif" font-size="17" letter-spacing="7">NOOR</text>
    <text x="88" y="848" font-family="Arial, sans-serif" font-size="8" letter-spacing="4" opacity="0.66">FABRIC STUDY · ${kind.replace('-', ' ').toUpperCase()}</text>
    <text x="88" y="882" font-family="Georgia, 'Times New Roman', serif" font-size="24">A closer look</text>
  </g>
</svg>`;
  }

  return `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 1000" width="800" height="1000">
  ${defs(p, 1)}
  <rect width="800" height="1000" fill="url(#bg1)"/>
  ${frame(p, 1)}
  ${figure(p, 1, kind)}
  <rect width="800" height="1000" filter="url(#grain1)" opacity="0.045"/>
  ${signature(p, safeName, kind.replace('-', ' '), '01', 1)}
</svg>`;
};

let index = 0;
for (const [name, kind, kicker, paletteIndex] of products) {
  index++;
  const slug = slugify(name);
  const p = palettes[paletteIndex];
  fs.writeFileSync(path.join(productDir, `${slug}-1.svg`), productSvg(name, kind, p, 1));
  fs.writeFileSync(path.join(productDir, `${slug}-2.svg`), productSvg(name, kind, p, 2));
}

const categories = [
  ['Burqas', 'burqas', 'Quietly considered coverage', 0],
  ['Abayas', 'abayas', 'Layer with ease', 1],
  ['Hijabs & Scarves', 'hijabs', 'Texture, colour, drape', 5],
  ['Niqabs', 'niqabs', 'Soft, seamless coverage', 3],
  ['Prayer Wear', 'prayer-wear', 'Ritual, made comfortable', 2],
  ['Accessories', 'accessories', 'The finishing details', 4]
];
for (const [name, slug, line, paletteIndex] of categories) {
  const p = palettes[paletteIndex];
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 900 1100" width="900" height="1100">
  ${defs(p, 7)}
  <rect width="900" height="1100" fill="url(#bg7)"/>
  <circle cx="720" cy="220" r="140" fill="${p.accent}" opacity="0.12"/>
  <path d="M170 950 L170 460 A 280 280 0 0 1 730 460 L730 950" fill="none" stroke="${p.soft}" stroke-opacity="0.8" stroke-width="1.2"/>
  <g transform="translate(50 0)">
    ${figure(p, 7, slug)}
  </g>
  <rect width="900" height="1100" filter="url(#grain7)" opacity="0.05"/>
  <g fill="${p.ink}">
    <text x="60" y="80" font-family="Georgia, 'Times New Roman', serif" font-size="17" letter-spacing="7">NOOR</text>
    <line x1="60" y1="960" x2="840" y2="960" stroke="${p.ink}" stroke-opacity="0.2"/>
    <text x="60" y="1000" font-family="Arial, sans-serif" font-size="9" letter-spacing="4" opacity="0.6">${line.toUpperCase()}</text>
    <text x="60" y="1048" font-family="Georgia, 'Times New Roman', serif" font-size="40">${name}</text>
    <text x="840" y="1048" text-anchor="end" font-family="Georgia, 'Times New Roman', serif" font-size="15" opacity="0.6">COLLECTION</text>
  </g>
</svg>`;
  fs.writeFileSync(path.join(categoryDir, `${slug}.svg`), svg);
}

const hero = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1200 1400" width="1200" height="1400">
  ${defs(palettes[0], 9)}
  <rect width="1200" height="1400" fill="url(#bg9)"/>
  <circle cx="970" cy="250" r="200" fill="${palettes[0].accent}" opacity="0.12"/>
  <circle cx="970" cy="250" r="154" fill="none" stroke="${palettes[0].accent}" stroke-opacity="0.25" stroke-width="1"/>
  <path d="M230 1310 L230 510 A 370 370 0 0 1 970 510 L970 1310" fill="none" stroke="${palettes[0].soft}" stroke-opacity="0.85" stroke-width="1.5"/>
  <path d="M198 1310 L198 500 A 402 402 0 0 1 1002 500 L1002 1310" fill="none" stroke="${palettes[0].accent}" stroke-opacity="0.22" stroke-width="1"/>
  <g transform="translate(200 110) scale(1.0)">
    ${figure(palettes[0], 9, 'burqas').replace('opacity="0.97"', 'opacity="0.99"')}
  </g>
  <path d="M0 1120 C 300 1060 900 1060 1200 1120 L1200 1400 L0 1400 Z" fill="${palettes[0].bg1}" opacity="0.28"/>
  <rect width="1200" height="1400" filter="url(#grain9)" opacity="0.05"/>
  <g fill="${palettes[0].ink}">
    <text x="72" y="96" font-family="Georgia, 'Times New Roman', serif" font-size="21" letter-spacing="9">NOOR</text>
    <text x="72" y="124" font-family="Arial, sans-serif" font-size="9" letter-spacing="4" opacity="0.6">MODEST ATELIER · EST. 2026</text>
    <text x="1128" y="1304" text-anchor="end" font-family="Arial, sans-serif" font-size="10" letter-spacing="4" opacity="0.55">THE AUTUMN EDIT</text>
  </g>
</svg>`;
fs.writeFileSync(path.join(editorialDir, 'hero.svg'), hero);

const story = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1000 1200" width="1000" height="1200">
  ${defs(palettes[1], 10)}
  <rect width="1000" height="1200" fill="url(#bg10)"/>
  <g opacity="0.97">
    <path d="M0 980 C 180 760 290 430 500 280 C 710 430 820 760 1000 980 L 1000 1200 L 0 1200 Z" fill="url(#fabric10)"/>
    <path d="M0 1060 C 260 860 330 560 500 430 C 670 560 740 860 1000 1060" fill="none" stroke="${palettes[1].soft}" stroke-opacity="0.3" stroke-width="2"/>
    <path d="M0 1140 C 300 960 390 700 500 620 C 610 700 700 960 1000 1140" fill="none" stroke="${palettes[1].soft}" stroke-opacity="0.16" stroke-width="2"/>
    <circle cx="760" cy="300" r="130" fill="none" stroke="${palettes[1].accent}" stroke-opacity="0.3"/>
    <circle cx="760" cy="300" r="96" fill="${palettes[1].accent}" opacity="0.12"/>
  </g>
  <rect width="1000" height="1200" filter="url(#grain10)" opacity="0.05"/>
  <g fill="${palettes[1].bg1}">
    <text x="70" y="92" font-family="Georgia, 'Times New Roman', serif" font-size="19" letter-spacing="8">NOOR</text>
    <text x="70" y="1092" font-family="Arial, sans-serif" font-size="9" letter-spacing="4" opacity="0.65">MADE SLOWLY · WORN OFTEN</text>
    <text x="70" y="1136" font-family="Georgia, 'Times New Roman', serif" font-size="38">The feel of quiet luxury.</text>
  </g>
</svg>`;
fs.writeFileSync(path.join(editorialDir, 'story.svg'), story);

const craft = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1000 700" width="1000" height="700">
  ${defs(palettes[2], 11)}
  <rect width="1000" height="700" fill="url(#bg11)"/>
  <circle cx="760" cy="160" r="120" fill="${palettes[2].accent}" opacity="0.13"/>
  <g>
    <path d="M220 560 C 320 480 380 400 500 340 C 620 400 680 480 780 560" fill="none" stroke="${palettes[2].fabric1}" stroke-opacity="0.7" stroke-width="26" stroke-linecap="round"/>
    <path d="M220 560 C 330 500 400 470 500 444 C 600 470 670 500 780 560" fill="none" stroke="${palettes[2].fabric2}" stroke-opacity="0.5" stroke-width="2"/>
    <path d="M270 590 C 370 540 430 520 500 502 C 570 520 630 540 730 590" fill="none" stroke="${palettes[2].fabric2}" stroke-opacity="0.35" stroke-width="2"/>
    <circle cx="500" cy="340" r="52" fill="${palettes[2].accent}" opacity="0.25"/>
    <circle cx="500" cy="340" r="34" fill="none" stroke="${palettes[2].accent}" stroke-opacity="0.55"/>
  </g>
  <rect width="1000" height="700" filter="url(#grain11)" opacity="0.05"/>
  <g fill="${palettes[2].ink}">
    <text x="60" y="76" font-family="Georgia, 'Times New Roman', serif" font-size="18" letter-spacing="8">NOOR</text>
    <text x="60" y="640" font-family="Georgia, 'Times New Roman', serif" font-size="32">Considered in every detail.</text>
  </g>
</svg>`;
fs.writeFileSync(path.join(editorialDir, 'craft.svg'), craft);

const mark = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64"><rect width="64" height="64" rx="14" fill="#1c1b19"/><path d="M18 44V25a14 14 0 0 1 28 0v19" fill="none" stroke="#c8b49a" stroke-width="2.4"/><path d="M24 44V27a8 8 0 0 1 16 0v17" fill="none" stroke="#a8834b" stroke-width="1.4"/><circle cx="32" cy="14" r="2" fill="#c8b49a"/></svg>`;
fs.writeFileSync(path.join(editorialDir, 'noor-mark.svg'), mark);

console.log('Generated', products.length * 2, 'product images,', categories.length, 'category images and 4 editorial assets.');

/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Components/**/*.razor',
    './wwwroot/**/*.html'
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        brand: {
          50: '#f0f7ff',
          100: '#e0effe',
          500: '#3b82f6',
          600: '#2563eb',
          700: '#1d4ed8',
          900: '#0f172a'
        },
        nocturne: {
          canvas: '#0A1B2E',
          card: '#112A46',
          elevated: '#1B3B5A',
          border: 'rgba(196, 210, 225, 0.12)',
          'border-focus': 'rgba(196, 210, 225, 0.35)',
          'text-primary': '#E1E8F0',
          'text-secondary': '#C4D2E1',
          'text-muted': '#7A8CA6'
        }
      }
    },
  },
  plugins: [],
}
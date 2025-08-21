/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './index.html',
    './src/**/*.{vue,js}',
    './node_modules/element-plus/dist/index.css'
  ],
  theme: {
    extend: {
      colors: {
        brand: {
          DEFAULT: '#409EFF'
        }
      },
      boxShadow: {
        card: '0 1px 2px rgba(0,0,0,0.05), 0 1px 3px rgba(0,0,0,0.1)'
      },
      borderRadius: {
        xl: '12px'
      }
    }
  },
  corePlugins: {
    preflight: true
  }
}



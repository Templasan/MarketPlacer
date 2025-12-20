/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}", // <--- Importante: Monitora todo HTML e TS dentro de src
  ],
  theme: {
    extend: {
      // Aqui estenderemos as cores futuramente para bater com o Angular Material
      colors: {
        // Exemplo: 'primary': '#3f51b5',
      },
      screens: {
        // Breakpoints padrão do Tailwind, compatíveis com Bootstrap se precisar
        'sm': '640px',
        'md': '768px',
        'lg': '1024px',
        'xl': '1280px',
        '2xl': '1536px',
      }
    },
  },
  plugins: [],
}
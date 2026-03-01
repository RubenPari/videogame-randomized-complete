/**
 * Tailwind CSS configuration for the Random Video Game Generator project
 *
 * Tailwind CSS is a utility-first CSS framework that provides predefined classes
 * to build modern user interfaces without writing custom CSS.
 *
 * This configuration defines:
 * - Which files should be scanned for Tailwind classes
 * - Theme customizations (colors, spacing, etc.)
 * - Additional plugins for extra functionality
 *
 * @type {import('tailwindcss').Config}
 */
export default {
  // Array of paths where Tailwind should look for used CSS classes
  // This "purging" process removes unused classes from the final bundle
  content: [
    "./index.html",                        // Main HTML file
    "./src/**/*.{vue,js,ts,jsx,tsx}",     // All component files in the src folder
  ],

  // Theme configuration - customizes colors, dimensions, fonts, etc.
  theme: {
    // The extend section allows adding new values without overriding defaults
    extend: {
      // Currently no theme customizations
      // Here you could add:
      // - Custom brand colors
      // - Custom fonts
      // - Custom responsive breakpoints
      // - Custom animations and transitions
    },
  },

  // Array of Tailwind plugins to add extra functionality
  plugins: [
    // Currently no plugins installed
    // Common plugins include:
    // - @tailwindcss/forms (for improved form styles)
    // - @tailwindcss/typography (for rich text content)
    // - @tailwindcss/aspect-ratio (for responsive aspect ratios)
  ],
}
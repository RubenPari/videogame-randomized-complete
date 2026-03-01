/**
 * PostCSS configuration for the Random Video Game Generator project
 *
 * PostCSS is a tool for transforming CSS with JavaScript plugins.
 * It is used to process CSS files during the build process.
 *
 * In this project, PostCSS is configured to use:
 * - Tailwind CSS: to generate utility classes
 * - Autoprefixer: to automatically add vendor prefixes
 *
 * This configuration is used by Vite during the build process
 * to process all CSS files and ensure cross-browser compatibility.
 */
export default {
  // Object containing the PostCSS plugins to use
  plugins: {
    // Tailwind CSS plugin to process @tailwind directives
    // Generates all utility classes based on the configuration in tailwind.config.js
    tailwindcss: {},

    // Autoprefixer plugin to automatically add vendor prefixes
    // Ensures compatibility with older browsers by adding prefixes like -webkit-, -moz-, etc.
    // The list of supported browsers is determined by .browserslistrc file or package.json
    autoprefixer: {},
  },
}
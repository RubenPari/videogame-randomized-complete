# Use Node.js LTS (Alpine for smaller image size)
FROM node:20-alpine

# Set the working directory inside the container
WORKDIR /app

# Copy package configuration to install dependencies
COPY package*.json ./

# Install project dependencies (npm ci is faster and more reliable)
RUN npm ci

# Copy the rest of the application files
COPY . .

# Expose Vite's default dev server port
EXPOSE 5173

# Run the dev server and bind to all host interfaces (0.0.0.0)
CMD ["npm", "run", "dev", "--", "--host"]

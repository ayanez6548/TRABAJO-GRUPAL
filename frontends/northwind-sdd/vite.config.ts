import { defineConfig } from 'vitest/config'
import { loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig(({ mode }) => {
  const environment = loadEnv(mode, process.cwd(), '')
  return {
    plugins: [react()],
    server: {
      host: '127.0.0.1',
      port: 5300,
      strictPort: true,
      proxy: {
        '/api': {
          target: environment.API_TARGET || 'http://127.0.0.1:5100',
          changeOrigin: true,
          rewrite: (path) => path.replace(/^\/api/, ''),
        },
      },
    },
    test: { environment: 'node', include: ['src/**/*.test.ts'], restoreMocks: true },
  }
})

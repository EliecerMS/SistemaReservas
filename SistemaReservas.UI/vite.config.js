import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
//import basicSsl from '@vitejs/plugin-basic-ssl'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(),
    //basicSsl()
  ],
  // run the dev server over HTTPS so browsers will accept the
  // `Secure` auth cookie sent by the API.  the CLI flag
  // (`npm run dev -- --https`) only works if the config enables
  // an https server; otherwise Vite exits with code 1 like you saw.
  /*server: {
    https: true,
    host: 'localhost',
    port: 5173*/
  // If you prefer to use a locally‑trusted certificate (recommended
  // for development), generate one with mkcert and point at the files:
  // https: {
  //   key: fs.readFileSync('./cert/localhost-key.pem'),
  //   cert: fs.readFileSync('./cert/localhost.pem')
  // }
}
  //}
)

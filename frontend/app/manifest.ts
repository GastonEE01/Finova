
import type { MetadataRoute } from "next";

export const dynamic = "force-static";

export default function manifest(): MetadataRoute.Manifest {
  return {
    name: "Finova",
    short_name: "Finova",
    description: "Gestioná tus finanzas personales con Finova.",
    start_url: "/",
    display: "standalone",
    background_color: "#ffffff",
    theme_color: "#121212",
    icons: [
      {
        src: "/Finova-512.png",
        sizes: "512x512",
        type: "image/png",
      },
      {
        src: "/Finova-192.png",
        sizes: "192x192",
        type: "image/png",
      },
    ],
  };
}

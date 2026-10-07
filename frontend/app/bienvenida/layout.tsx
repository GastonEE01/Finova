import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Finova — Sabé en qué se va tu plata",
  description: "Registrá ingresos y gastos en segundos, mirá gráficos de tu mes y preguntale a tu asistente financiero. Gratis y en español.",
};

export default function BienvenidaLayout({ children }: { children: React.ReactNode }) {
  return children;
}

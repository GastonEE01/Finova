"use client";

import { useTheme } from "@mui/material";

// Paleta de gráficos "Bosque y moneda", adaptada al modo claro/oscuro.
// Semántica fija: esmeralda = ingresos, rojo = gastos, oro = ahorro/destacado.
export function useChartPalette() {
  const { palette } = useTheme();
  const light = palette.mode === "light";

  return {
    income: light ? "#0E7A5F" : "#3AA586",
    expense: light ? "#C0392B" : "#E06C5B",
    gold: "#C99A2C",
    slices: light
      ? ["#0E7A5F", "#C99A2C", "#2E9E7B", "#B07A3F", "#5B8C7B", "#D9B45B", "#7FB69E", "#8C6E3C", "#476B5E", "#A88B4A"]
      : ["#3AA586", "#D9B45B", "#2E9E7B", "#C98F3F", "#7FB69E", "#E0C06A", "#5B8C7B", "#A88B4A", "#8FD0B4", "#C99A2C"],
  };
}

export function fmtMoney(currency: string) {
  return (v: number | null) => (v === null ? "" : `${v.toLocaleString("es-AR", { maximumFractionDigits: 2 })} ${currency}`);
}

export function fmtPie(currency: string) {
  return (v: { value: number }) => `${v.value.toLocaleString("es-AR", { maximumFractionDigits: 2 })} ${currency}`;
}

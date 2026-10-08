"use client";

import { Box, Typography } from "@mui/material";
import { BarChart, PieChart } from "@mui/x-charts";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { apiFetch, getToken } from "../../lib/auth";
import { useChartPalette, fmtMoney, fmtPie } from "../../components/chartPalette";

type CategoryItem = { categoryId?: string | null; categoryName: string; total: number };
type Response = { currency: string; items: CategoryItem[] };

export default function ExpensesByCategoryChart({ currency }: { currency: string }) {
  const router = useRouter();
  const palette = useChartPalette();
  const [data, setData] = useState<Response | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!getToken()) { router.push("/login"); return; }
    let cancelled = false;
    Promise.resolve().then(() => {
      if (cancelled) return;
      setData(null);
      setError("");
    });
    apiFetch(`/api/dashboard/expenses-by-category?currency=${encodeURIComponent(currency)}`)
      .then(async (res) => {
        if (cancelled) return;
        if (res.status === 401) { router.push("/login"); return; }
        if (res.ok) setData(await res.json());
        else setError("No se pudo cargar el gráfico.");
      })
      .catch(() => { if (!cancelled) setError("No se pudo cargar el gráfico."); });
    return () => { cancelled = true; };
  }, [currency, router]);

  return (
    <Box>
      <Typography variant="subtitle1" sx={{ fontWeight: "bold" }}>Gastos por categoría del mes</Typography>
      {error && <Typography color="error">{error}</Typography>}
      {!error && !data && <Typography color="text.secondary">Cargando gráfico...</Typography>}
      {data && data.items.length === 0 && (
        <Typography color="text.secondary">Sin movimientos en el período.</Typography>
      )}
      {data && data.items.length > 0 && (
        data.items.length > 8 ? (
          <BarChart
            height={280}
            layout="horizontal"
            borderRadius={6}
            yAxis={[{ scaleType: "band", data: data.items.map((i) => i.categoryName) }]}
            series={[{ data: data.items.map((i) => i.total), label: `Gastos (${currency})`, color: palette.expense, valueFormatter: fmtMoney(currency) }]}
          />
        ) : (
          <PieChart
            height={280}
            series={[{
              data: data.items.map((i, idx) => ({ id: idx, label: i.categoryName, value: i.total, color: palette.slices[idx % palette.slices.length] })),
              innerRadius: 70,
              paddingAngle: 2,
              cornerRadius: 5,
              valueFormatter: fmtPie(currency),
            }]}
          />
        )
      )}
    </Box>
  );
}

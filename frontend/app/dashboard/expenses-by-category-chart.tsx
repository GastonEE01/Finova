"use client";

import { Box, Typography } from "@mui/material";
import { BarChart, PieChart } from "@mui/x-charts";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { apiFetch, getToken } from "../lib/auth";

type CategoryItem = { categoryId?: string | null; categoryName: string; total: number };
type Response = { currency: string; items: CategoryItem[] };

export default function ExpensesByCategoryChart({ currency }: { currency: string }) {
  const router = useRouter();
  const [data, setData] = useState<Response | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!getToken()) { router.push("/login"); return; }
    setData(null);
    setError("");
    apiFetch(`/api/dashboard/expenses-by-category?currency=${encodeURIComponent(currency)}`)
      .then(async (res) => {
        if (res.status === 401) { router.push("/login"); return; }
        if (res.ok) setData(await res.json());
        else setError("No se pudo cargar el gráfico.");
      })
      .catch(() => setError("No se pudo cargar el gráfico."));
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
            yAxis={[{ scaleType: "band", data: data.items.map((i) => i.categoryName) }]}
            series={[{ data: data.items.map((i) => i.total), label: `Gastos (${currency})` }]}
          />
        ) : (
          <PieChart
            height={280}
            series={[{
              data: data.items.map((i, idx) => ({ id: idx, label: i.categoryName, value: i.total })),
            }]}
          />
        )
      )}
    </Box>
  );
}

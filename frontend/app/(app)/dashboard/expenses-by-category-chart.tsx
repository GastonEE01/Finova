"use client";

import { Box, Typography, useTheme } from "@mui/material";
import { Bar, BarChart, CartesianGrid, Cell, Legend, Pie, PieChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { apiFetch, getToken } from "../../lib/auth";
import { useChartPalette } from "../../components/chartPalette";

type CategoryItem = { categoryId?: string | null; categoryName: string; total: number };
type Response = { currency: string; items: CategoryItem[] };

const tooltipStyle = { borderRadius: 8, fontSize: 13 };

export default function ExpensesByCategoryChart({ currency, year, month }: { currency: string; year: number; month: number }) {
  const router = useRouter();
  const palette = useChartPalette();
  const { palette: mui } = useTheme();
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
    apiFetch(`/api/dashboard/expenses-by-category?currency=${encodeURIComponent(currency)}&year=${year}&month=${month}`)
      .then(async (res) => {
        if (cancelled) return;
        if (res.status === 401) { router.push("/login"); return; }
        if (res.ok) setData(await res.json());
        else setError("No se pudo cargar el gráfico.");
      })
      .catch(() => { if (!cancelled) setError("No se pudo cargar el gráfico."); });
    return () => { cancelled = true; };
  }, [currency, year, month, router]);

  const fmt = (v: number) => `${v.toLocaleString("es-AR", { maximumFractionDigits: 2 })} ${currency}`;
  const pieData = data ? data.items.map((i) => ({ name: i.categoryName, value: i.total })) : [];

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
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={pieData} layout="vertical" margin={{ left: 8, right: 16 }}>
              <CartesianGrid strokeDasharray="3 3" stroke={mui.divider} horizontal={false} />
              <XAxis type="number" tick={{ fontSize: 12, fill: mui.text.secondary }} />
              <YAxis type="category" dataKey="name" width={110} tick={{ fontSize: 12, fill: mui.text.secondary }} />
              <Tooltip
                formatter={(v) => [fmt(Number(v)), "Gastos"]}
                contentStyle={tooltipStyle}
                cursor={{ fill: mui.action.hover, opacity: 0.4 }}
              />
              <Bar dataKey="value" fill={palette.expense} radius={[0, 6, 6, 0]} />
            </BarChart>
          </ResponsiveContainer>
        ) : (
          <Box>
            <ResponsiveContainer width="100%" height={260}>
              <PieChart>
                <Pie
                  data={pieData}
                  dataKey="value"
                  nameKey="name"
                  innerRadius={60}
                  outerRadius={95}
                  paddingAngle={2}
                  cornerRadius={5}
                >
                  {pieData.map((_, idx) => (
                    <Cell key={idx} fill={palette.slices[idx % palette.slices.length]} />
                  ))}
                </Pie>
                <Tooltip formatter={(v) => [fmt(Number(v)), "Gastos"]} contentStyle={tooltipStyle} />
                <Legend wrapperStyle={{ fontSize: 12 }} />
              </PieChart>
            </ResponsiveContainer>
            <Box sx={{ display: "flex", flexDirection: "column", gap: 0.5, mt: 1 }}>
              {data.items.map((i) => (
                <Typography key={`${i.categoryId ?? i.categoryName}`} variant="body2" color="text.secondary">
                  {i.categoryName} · {fmt(i.total)}
                </Typography>
              ))}
            </Box>
          </Box>
        )
      )}
    </Box>
  );
}

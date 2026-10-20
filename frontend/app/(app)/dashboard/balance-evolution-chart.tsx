"use client";

import { Box, Typography, useTheme } from "@mui/material";
import { Area, AreaChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { apiFetch, getToken } from "../../lib/auth";
import { useChartPalette } from "../../components/chartPalette";

type PointItem = { date: string; balance: number };
type Response = { currency: string; points: PointItem[] };

const tooltipStyle = { borderRadius: 8, fontSize: 13 };

export default function BalanceEvolutionChart({ currency, year, month }: { currency: string; year: number; month: number }) {
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
    apiFetch(`/api/dashboard/balance-evolution?currency=${encodeURIComponent(currency)}&year=${year}&month=${month}`)
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
  const chartData = data ? data.points.map((p) => ({ dia: new Date(p.date).getDate(), Saldo: p.balance })) : [];

  return (
    <Box>
      <Typography variant="subtitle1" sx={{ fontWeight: "bold" }}>Evolución del saldo del mes</Typography>
      {error && <Typography color="error">{error}</Typography>}
      {!error && !data && <Typography color="text.secondary">Cargando gráfico...</Typography>}
      {data && data.points.length === 0 && (
        <Typography color="text.secondary">Sin movimientos en el período.</Typography>
      )}
      {data && data.points.length > 0 && (
        <ResponsiveContainer width="100%" height={300}>
          <AreaChart data={chartData} margin={{ top: 8, right: 8 }}>
            <defs>
              <linearGradient id="gradSaldo" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor={palette.income} stopOpacity={0.45} />
                <stop offset="100%" stopColor={palette.income} stopOpacity={0.02} />
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="3 3" stroke={mui.divider} vertical={false} />
            <XAxis dataKey="dia" tick={{ fontSize: 12, fill: mui.text.secondary }} />
            <YAxis tick={{ fontSize: 12, fill: mui.text.secondary }} width={70} />
            <Tooltip
              formatter={(v) => [fmt(Number(v)), "Saldo"]}
              labelFormatter={(label) => `Día ${label}`}
              contentStyle={tooltipStyle}
            />
            <Area
              type="monotone"
              dataKey="Saldo"
              stroke={palette.income}
              strokeWidth={2.5}
              fill="url(#gradSaldo)"
            />
          </AreaChart>
        </ResponsiveContainer>
      )}
    </Box>
  );
}

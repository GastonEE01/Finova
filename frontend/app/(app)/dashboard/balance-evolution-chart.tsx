"use client";

import { Box, Typography } from "@mui/material";
import { LineChart } from "@mui/x-charts";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { apiFetch, getToken } from "../../lib/auth";
import { useChartPalette, fmtMoney } from "../../components/chartPalette";

type PointItem = { date: string; balance: number };
type Response = { currency: string; points: PointItem[] };

export default function BalanceEvolutionChart({ currency }: { currency: string }) {
  const router = useRouter();
  const palette = useChartPalette();
  const [data, setData] = useState<Response | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!getToken()) { router.push("/login"); return; }
    setData(null);
    setError("");
    apiFetch(`/api/dashboard/balance-evolution?currency=${encodeURIComponent(currency)}`)
      .then(async (res) => {
        if (res.status === 401) { router.push("/login"); return; }
        if (res.ok) setData(await res.json());
        else setError("No se pudo cargar el gráfico.");
      })
      .catch(() => setError("No se pudo cargar el gráfico."));
  }, [currency, router]);

  return (
    <Box>
      <Typography variant="subtitle1" sx={{ fontWeight: "bold" }}>Evolución del saldo del mes</Typography>
      {error && <Typography color="error">{error}</Typography>}
      {!error && !data && <Typography color="text.secondary">Cargando gráfico...</Typography>}
      {data && data.points.length === 0 && (
        <Typography color="text.secondary">Sin movimientos en el período.</Typography>
      )}
      {data && data.points.length > 0 && (
        <LineChart
          height={280}
          xAxis={[{
            scaleType: "point",
            data: data.points.map((p) => new Date(p.date).getDate()),
          }]}
          series={[{ data: data.points.map((p) => p.balance), label: `Saldo (${currency})`, color: palette.income, area: true, showMark: false, curve: "monotoneX", valueFormatter: fmtMoney(currency) }]}
        />
      )}
    </Box>
  );
}

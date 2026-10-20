"use client";

import { Box, Typography } from "@mui/material";
import { Cell, Legend, Pie, PieChart, ResponsiveContainer, Tooltip } from "recharts";
import { useChartPalette } from "../../components/chartPalette";

export type TopCategory = { categoryId?: string | null; categoryName: string; total: number; percent: number };

const tooltipStyle = { borderRadius: 8, fontSize: 13 };

export default function TopCategoriesChart({ items, currency }: { items: TopCategory[]; currency: string }) {
  const palette = useChartPalette();
  const fmt = (v: number) => `${v.toLocaleString("es-AR", { maximumFractionDigits: 2 })} ${currency}`;
  const data = items.map((i) => ({ name: i.categoryName, value: i.total }));

  return (
    <Box>
      <Typography variant="subtitle1" sx={{ fontWeight: "bold" }}>Dónde gastás más (top 5)</Typography>
      {items.length === 0 ? (
        <Typography color="text.secondary">Sin gastos este mes.</Typography>
      ) : (
        <>
          <ResponsiveContainer width="100%" height={260}>
            <PieChart>
              <Pie
                data={data}
                dataKey="value"
                nameKey="name"
                innerRadius={55}
                outerRadius={90}
                paddingAngle={2}
                cornerRadius={5}
              >
                {data.map((_, idx) => (
                  <Cell key={idx} fill={palette.slices[idx % palette.slices.length]} />
                ))}
              </Pie>
              <Tooltip formatter={(v) => [fmt(Number(v)), "Gastos"]} contentStyle={tooltipStyle} />
              <Legend wrapperStyle={{ fontSize: 12 }} />
            </PieChart>
          </ResponsiveContainer>
          <Box sx={{ display: "flex", flexDirection: "column", gap: 0.5, mt: 1 }}>
            {items.map((i) => (
              <Typography key={`${i.categoryId ?? i.categoryName}`} variant="body2" color="text.secondary">
                {i.categoryName} · {fmt(i.total)} · {i.percent.toFixed(2)}%
              </Typography>
            ))}
          </Box>
        </>
      )}
    </Box>
  );
}

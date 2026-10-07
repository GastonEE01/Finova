"use client";

import { Box, Typography } from "@mui/material";
import { PieChart } from "@mui/x-charts";
import { useChartPalette, fmtPie } from "../../components/chartPalette";

export type TopCategory = { categoryId?: string | null; categoryName: string; total: number; percent: number };

export default function TopCategoriesChart({ items, currency }: { items: TopCategory[]; currency: string }) {
  const palette = useChartPalette();
  return (
    <Box>
      <Typography variant="subtitle1" sx={{ fontWeight: "bold" }}>Dónde gastás más (top 5)</Typography>
      {items.length === 0 ? (
        <Typography color="text.secondary">Sin gastos este mes.</Typography>
      ) : (
        <>
          <PieChart
            height={280}
            series={[{
              data: items.map((i, idx) => ({ id: idx, label: `${i.categoryName} (${i.percent.toFixed(2)}%)`, value: i.total, color: palette.slices[idx % palette.slices.length] })),
              innerRadius: 70,
              paddingAngle: 2,
              cornerRadius: 5,
              valueFormatter: fmtPie(currency),
            }]}
          />
          <Box sx={{ display: "flex", flexDirection: "column", gap: 0.5, mt: 1 }}>
            {items.map((i) => (
              <Typography key={`${i.categoryId ?? i.categoryName}`} variant="body2">
                {i.categoryName} · {i.total.toFixed(2)} {currency} · {i.percent.toFixed(2)}%
              </Typography>
            ))}
          </Box>
        </>
      )}
    </Box>
  );
}

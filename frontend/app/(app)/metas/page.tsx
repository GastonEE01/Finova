"use client";

import { Box, Button, Chip, Container, Dialog, DialogActions, DialogContent, DialogTitle, LinearProgress, MenuItem, TextField, Typography } from "@mui/material";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { apiFetch, getToken } from "../../lib/auth";

type Goal = { id: string; name: string; targetAmount: number; progress: number; remaining: number; percent: number; status: string; targetDate: string; createdAt: string };
type Account = { id: string; name: string; currency: string; balance: number };
type Category = { id: string; name: string; type: number };

const estadoColor: Record<string, "success" | "info" | "error"> = { Cumplida: "success", "En curso": "info", Vencida: "error" };

export default function MetasPage() {
  const router = useRouter();
  const [goals, setGoals] = useState<Goal[]>([]);
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<Goal | null>(null);
  const [name, setName] = useState("");
  const [targetAmount, setTargetAmount] = useState("");
  const [targetDate, setTargetDate] = useState("");
  const [error, setError] = useState("");
  const [contribOpen, setContribOpen] = useState(false);
  const [contribGoal, setContribGoal] = useState<Goal | null>(null);
  const [accountId, setAccountId] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [contribAmount, setContribAmount] = useState("");
  const [contribError, setContribError] = useState("");

  const load = async () => {
    const res = await apiFetch("/api/savinggoals");
    if (res.status === 401) { router.push("/login"); return; }
    if (res.ok) setGoals(await res.json());
  };

  const loadHelpers = async () => {
    const [accRes, catRes] = await Promise.all([
      apiFetch("/api/accounts"),
      apiFetch("/api/categories?type=2"),
    ]);
    if (accRes.ok) setAccounts(await accRes.json());
    if (catRes.ok) setCategories(await catRes.json());
  };

  useEffect(() => {
    if (!getToken()) { router.push("/login"); return; }
    load();
    loadHelpers();
  }, [router]);

  const openCreate = () => {
    setEditing(null); setName(""); setTargetAmount(""); setTargetDate(""); setError(""); setOpen(true);
  };

  const openEdit = (g: Goal) => {
    setEditing(g); setName(g.name); setTargetAmount(String(g.targetAmount));
    setTargetDate(g.targetDate.slice(0, 10)); setError(""); setOpen(true);
  };

  const handleSave = async () => {
    setError("");
    const monto = Number(targetAmount);
    if (!name.trim()) { setError("El nombre es obligatorio."); return; }
    if (!monto || monto <= 0) { setError("El monto objetivo debe ser mayor a 0."); return; }
    if (!targetDate) { setError("La fecha objetivo es obligatoria."); return; }
    const body = { name: name.trim(), targetAmount: monto, targetDate };
    const res = editing
      ? await apiFetch(`/api/savinggoals/${editing.id}`, { method: "PUT", body: JSON.stringify(body) })
      : await apiFetch("/api/savinggoals", { method: "POST", body: JSON.stringify(body) });
    if (res.ok) { setOpen(false); load(); }
    else {
      try { const data = await res.json(); setError(data.mensaje ?? "No se pudo guardar la meta."); }
      catch { setError("No se pudo guardar la meta."); }
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm("¿Eliminar esta meta? Sus aportes se conservan como gastos normales.")) return;
    const res = await apiFetch(`/api/savinggoals/${id}`, { method: "DELETE" });
    if (res.ok) load();
  };

  const openContrib = (g: Goal) => {
    setContribGoal(g); setAccountId(""); setCategoryId(""); setContribAmount(""); setContribError(""); setContribOpen(true);
  };

  const handleContrib = async () => {
    setContribError("");
    const monto = Number(contribAmount);
    if (!accountId) { setContribError("Elegí una cuenta."); return; }
    if (!categoryId) { setContribError("Elegí una categoría de gasto."); return; }
    if (!monto || monto <= 0) { setContribError("El monto debe ser mayor a 0."); return; }
    const res = await apiFetch(`/api/savinggoals/${contribGoal!.id}/contributions`, {
      method: "POST",
      body: JSON.stringify({ accountId, categoryId, amount: monto }),
    });
    if (res.ok) { setContribOpen(false); load(); }
    else {
      try { const data = await res.json(); setContribError(data.mensaje ?? "No se pudo registrar el aporte."); }
      catch { setContribError("No se pudo registrar el aporte."); }
    }
  };

  return (
    <Container maxWidth="sm">
      <Box sx={{ mt: 4, display: "flex", flexDirection: "column", gap: 2 }}>
        <Typography variant="h5">Metas de ahorro</Typography>
        <Button variant="contained" onClick={openCreate}>Nueva meta</Button>
        {goals.map((g) => (
          <Box key={g.id} sx={{ border: 1, borderColor: "divider", borderRadius: 2, p: 2, display: "flex", flexDirection: "column", gap: 1 }}>
            <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", gap: 1 }}>
              <Typography variant="subtitle1">{g.name}</Typography>
              <Chip label={g.status} color={estadoColor[g.status] ?? "default"} size="small" />
            </Box>
            <Typography variant="body2" color="text.secondary">
              Progreso {g.progress.toFixed(2)} / Objetivo {g.targetAmount.toFixed(2)} · Restante {g.remaining.toFixed(2)}
            </Typography>
            <LinearProgress variant="determinate" value={Math.min(g.percent, 100)} color={g.status === "Cumplida" ? "success" : g.status === "Vencida" ? "error" : "primary"} />
            <Typography variant="caption" color="text.secondary">
              {g.percent.toFixed(1)}% · Fecha objetivo: {g.targetDate.slice(0, 10)}
              {g.status === "Cumplida" && g.remaining < 0 ? ` · ¡Meta cumplida! Excedente: ${(-g.remaining).toFixed(2)}` : ""}
            </Typography>
            <Box sx={{ display: "flex", gap: 1, flexWrap: "wrap" }}>
              <Button size="small" variant="contained" onClick={() => openContrib(g)}>Aportar</Button>
              <Button size="small" onClick={() => openEdit(g)}>Editar</Button>
              <Button size="small" color="error" onClick={() => handleDelete(g.id)}>Eliminar</Button>
            </Box>
          </Box>
        ))}
        {goals.length === 0 && <Typography color="text.secondary">No tenés metas todavía.</Typography>}
      </Box>

      <Dialog open={open} onClose={() => setOpen(false)} fullWidth>
        <DialogTitle>{editing ? "Editar meta" : "Nueva meta"}</DialogTitle>
        <DialogContent sx={{ display: "flex", flexDirection: "column", gap: 2, pt: 2 }}>
          <TextField label="Nombre" value={name} onChange={(e) => setName(e.target.value)} fullWidth />
          <TextField label="Monto objetivo" type="number" value={targetAmount} onChange={(e) => setTargetAmount(e.target.value)} fullWidth />
          <TextField label="Fecha objetivo" type="date" value={targetDate} onChange={(e) => setTargetDate(e.target.value)} slotProps={{ inputLabel: { shrink: true } }} fullWidth />
          {error && <Typography color="error">{error}</Typography>}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)}>Cancelar</Button>
          <Button variant="contained" onClick={handleSave}>Guardar</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={contribOpen} onClose={() => setContribOpen(false)} fullWidth>
        <DialogTitle>Aportar a {contribGoal?.name}</DialogTitle>
        <DialogContent sx={{ display: "flex", flexDirection: "column", gap: 2, pt: 2 }}>
          <Typography variant="body2" color="text.secondary">El aporte descuenta de la cuenta elegida y se registra como gasto.</Typography>
          <TextField select label="Cuenta" value={accountId} onChange={(e) => setAccountId(e.target.value)} fullWidth>
            {accounts.map((a) => <MenuItem key={a.id} value={a.id}>{a.name} ({a.currency})</MenuItem>)}
          </TextField>
          <TextField select label="Categoría de gasto" value={categoryId} onChange={(e) => setCategoryId(e.target.value)} fullWidth>
            {categories.map((c) => <MenuItem key={c.id} value={c.id}>{c.name}</MenuItem>)}
          </TextField>
          <TextField label="Monto" type="number" value={contribAmount} onChange={(e) => setContribAmount(e.target.value)} fullWidth />
          {contribError && <Typography color="error">{contribError}</Typography>}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setContribOpen(false)}>Cancelar</Button>
          <Button variant="contained" onClick={handleContrib}>Aportar</Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

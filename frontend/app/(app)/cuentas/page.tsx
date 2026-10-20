"use client";

import { Box, Button, Container, Dialog, DialogActions, DialogContent, DialogTitle, FormControl, InputLabel, MenuItem, Select, TextField, Typography } from "@mui/material";
import { useRouter } from "next/navigation";
import { useCallback, useEffect, useState } from "react";
import { FiCreditCard, FiPlus } from "react-icons/fi";
import { apiFetch, getToken } from "../../lib/auth";
import { CURRENCIES } from "../../lib/currencies";

type Account = { id: string; name: string; currency: string; balance: number };

export default function CuentasPage() {
  const router = useRouter();
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [open, setOpen] = useState(false);
  const [name, setName] = useState("");
  const [currency, setCurrency] = useState("ARS");
  const [error, setError] = useState("");

  const load = useCallback(async () => {
    const res = await apiFetch("/api/accounts");
    if (res.status === 401) { router.push("/login"); return; }
    if (res.ok) setAccounts(await res.json());
  }, [router]);

  useEffect(() => {
    if (!getToken()) { router.push("/login"); return; }
    let cancelled = false;
    apiFetch("/api/accounts").then(async (res) => {
      if (cancelled) return;
      if (res.status === 401) { router.push("/login"); return; }
      if (res.ok) setAccounts(await res.json());
    });
    return () => { cancelled = true; };
  }, [router, load]);

  const handleCreate = async () => {
    setError("");
    const res = await apiFetch("/api/accounts", {
      method: "POST",
      body: JSON.stringify({ name, currency }),
    });
    if (res.ok) {
      setOpen(false);
      setName("");
      setCurrency("ARS");
      load();
    } else {
      setError("No se pudo crear la cuenta.");
    }
  };

  return (
    <Container maxWidth="sm">
      <Box sx={{ mt: 4, display: "flex", flexDirection: "column", gap: 2 }}>
        <Typography variant="h5">Mis cuentas</Typography>
        <Button variant="contained" startIcon={<FiPlus />} onClick={() => setOpen(true)}>Nueva cuenta</Button>

        <Box sx={{ display: "flex", flexDirection: "column", gap: 1.5 }}>
          {accounts.map((a) => (
            <Box
              key={a.id}
              sx={{
                display: "flex",
                alignItems: "center",
                gap: 2,
                p: 2,
                borderRadius: 3,
                border: "1px solid",
                borderColor: "divider",
                bgcolor: "background.paper",
                transition: "box-shadow 0.2s, border-color 0.2s",
                "&:hover": { boxShadow: 2, borderColor: "primary.main" },
              }}
            >
              <Box
                sx={{
                  width: 44,
                  height: 44,
                  borderRadius: 2,
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  bgcolor: "primary.main",
                  color: "#fff",
                  flexShrink: 0,
                }}
              >
                <FiCreditCard size={22} />
              </Box>

              <Box sx={{ flexGrow: 1, minWidth: 0 }}>
                <Typography variant="subtitle1" sx={{ fontWeight: 600, overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
                  {a.name}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {a.currency}
                </Typography>
              </Box>

              <Typography
                variant="h6"
                sx={{
                  fontWeight: 700,
                  whiteSpace: "nowrap",
                  color: a.balance < 0 ? "error.main" : "text.primary",
                }}
              >
                {a.balance.toLocaleString("es-AR", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
              </Typography>
            </Box>
          ))}
          {accounts.length === 0 && <Typography color="text.secondary">No tenés cuentas todavía.</Typography>}
        </Box>
      </Box>

      <Dialog open={open} onClose={() => setOpen(false)}>
        <DialogTitle>Nueva cuenta</DialogTitle>
        <DialogContent sx={{ display: "flex", flexDirection: "column", gap: 2, pt: 2 }}>
          <TextField label="Nombre" value={name} onChange={(e) => setName(e.target.value)} fullWidth />
          <FormControl fullWidth>
            <InputLabel>Moneda</InputLabel>
            <Select value={currency} label="Moneda" onChange={(e) => setCurrency(e.target.value)}>
              {CURRENCIES.map((c) => <MenuItem key={c.code} value={c.code}>{c.code} — {c.name}</MenuItem>)}
            </Select>
          </FormControl>
          {error && <Typography color="error">{error}</Typography>}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpen(false)}>Cancelar</Button>
          <Button variant="contained" onClick={handleCreate}>Crear</Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}

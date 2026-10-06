"use client";

import { Box, Button, Container, Typography } from "@mui/material";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { clearToken, getEmail, getToken } from "./lib/auth";

export default function Home() {
  const router = useRouter();
  const [email, setEmail] = useState<string | null>(null);

  useEffect(() => {
    if (!getToken()) {
      router.push("/login");
    } else {
      setEmail(getEmail());
    }
  }, [router]);

  const handleLogout = () => {
    clearToken();
    router.push("/login");
  };

  return (
    <Container maxWidth="sm">
      <Box sx={{ mt: 8, display: "flex", flexDirection: "column", alignItems: "center", gap: 2 }}>
        <Typography variant="h4">Finova</Typography>
        <Typography variant="body1" color="text.secondary">
          {email ? `Sesión: ${email}` : "Cargando..."}
        </Typography>
        <Box sx={{ display: "flex", gap: 2, flexWrap: "wrap", justifyContent: "center" }}>
          <Button variant="contained" href="/dashboard">Dashboard</Button>
          <Button variant="contained" href="/cuentas">Mis cuentas</Button>
          <Button variant="contained" href="/movimientos/nuevo">Nuevo ingreso</Button>
          <Button variant="contained" href="/movimientos/nuevo-gasto">Nuevo gasto</Button>
          <Button variant="contained" href="/movimientos">Historial</Button>
          <Button variant="contained" href="/presupuestos">Presupuestos</Button>
          <Button variant="contained" href="/metas">Metas de ahorro</Button>
          <Button variant="outlined" onClick={handleLogout}>Cerrar sesión</Button>
        </Box>
      </Box>
    </Container>
  );
}

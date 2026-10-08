"use client";

import { Box, Button, Container, TextField, Typography } from "@mui/material";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { setToken, API_URL } from "../lib/auth";

export default function RegistroPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  const handleRegistro = async () => {
    setError("");
    const res = await fetch(`${API_URL}/api/auth/register`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, password }),
    });
    if (res.ok) {
      const data = await res.json();
      setToken(data.token, data.email);
      router.push("/");
    } else if (res.status === 409) {
      setError("Ya existe un usuario con ese email.");
    } else {
      setError("No se pudo registrar. Revisá los datos.");
    }
  };

  return (
    <Container maxWidth="xs">
      <Box sx={{ mt: 8, display: "flex", flexDirection: "column", gap: 2 }}>
        <Typography variant="h5">Crear cuenta</Typography>
        <TextField label="Email" value={email} onChange={(e) => setEmail(e.target.value)} fullWidth />
        <TextField label="Contraseña (mín. 8)" type="password" value={password} onChange={(e) => setPassword(e.target.value)} fullWidth />
        {error && <Typography color="error">{error}</Typography>}
        <Button variant="contained" onClick={handleRegistro}>Registrarse</Button>
        <Button href="/login">Ya tengo cuenta</Button>
      </Box>
    </Container>
  );
}

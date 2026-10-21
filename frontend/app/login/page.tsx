"use client";

import { Box, Button, Container, TextField, Typography } from "@mui/material";
import Image from "next/image";
import { useRouter } from "next/navigation";
import { useState } from "react";
import PasswordField from "../components/PasswordField";
import { setToken, API_URL } from "../lib/auth";

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  const handleLogin = async () => {
    setError("");
    const res = await fetch(`${API_URL}/api/auth/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, password }),
    });
    if (res.ok) {
      const data = await res.json();
      setToken(data.token, data.email);
      router.push("/");
    } else {
      setError("Credenciales inválidas.");
    }
  };

  return (
    <Container maxWidth="xs">
      <Box sx={{ mt: 8, display: "flex", flexDirection: "column", gap: 2, alignItems: "center" }}>
        <Box
          component="button"
          type="button"
          aria-label="Ir al inicio"
          onClick={() => router.push("/bienvenida")}
          sx={{ p: 0, border: "none", bg: "transparent", cursor: "pointer", display: "flex", lineHeight: 0 }}
        >
          <Image src="/Logo.png" alt="Finova" width={88} height={88} priority style={{ borderRadius: 20, height: 88, width: "auto" }} />
        </Box>
        <Typography variant="h5">Iniciar sesión</Typography>
      </Box>
      <Box sx={{ mt: 2, display: "flex", flexDirection: "column", gap: 2 }}>
        <TextField label="Email" value={email} onChange={(e) => setEmail(e.target.value)} fullWidth />
        <PasswordField label="Contraseña" value={password} onChange={(e) => setPassword(e.target.value)} fullWidth />
        {error && <Typography color="error">{error}</Typography>}
        <Button variant="contained" onClick={handleLogin}>Entrar</Button>
        <Button href="/registro">Crear cuenta</Button>
      </Box>
    </Container>
  );
}

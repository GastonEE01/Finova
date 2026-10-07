"use client";

import { Box, Button, Container, Typography } from "@mui/material";
import { useRouter } from "next/navigation";
import { FiArrowRight, FiBarChart2, FiCreditCard, FiMessageCircle, FiPieChart, FiShield, FiTarget } from "react-icons/fi";

function Feature({ icon, title, text }: { icon: React.ReactNode; title: string; text: string }) {
  return (
    <Box sx={{ display: "flex", gap: 2, alignItems: "flex-start" }}>
      <Box sx={{ color: "primary.main", mt: 0.5 }}>{icon}</Box>
      <Box>
        <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>{title}</Typography>
        <Typography variant="body2" color="text.secondary">{text}</Typography>
      </Box>
    </Box>
  );
}

function PhoneMockup() {
  return (
    <Box
      sx={{
        width: 250, borderRadius: 6, border: 1, borderColor: "divider", bgcolor: "background.paper",
        p: 2, display: "flex", flexDirection: "column", gap: 1.5,
        boxShadow: "0 24px 48px rgba(18,33,27,0.18)",
      }}
    >
      <Typography variant="caption" color="text.secondary">Hola, gaston@…</Typography>
      <Typography variant="caption" color="text.secondary">Saldo total</Typography>
      <Typography variant="h5" sx={{ fontFamily: '"Sora", sans-serif', fontWeight: 700 }}>$ 72.000</Typography>
      <Typography variant="caption" sx={{ color: "success.main", fontWeight: 700 }}>+2,1% vs mes anterior</Typography>
      <Box sx={{ display: "flex", gap: 1 }}>
        <Box sx={{ flex: 1, borderRadius: 2, bgcolor: "primary.main", p: 1 }}>
          <Typography variant="caption" sx={{ color: "#fff" }}>Ingresos</Typography>
          <Typography variant="body2" sx={{ color: "#fff", fontWeight: 700 }}>$ 100.000</Typography>
        </Box>
        <Box sx={{ flex: 1, borderRadius: 2, bgcolor: "error.main", p: 1 }}>
          <Typography variant="caption" sx={{ color: "#fff" }}>Gastos</Typography>
          <Typography variant="body2" sx={{ color: "#fff", fontWeight: 700 }}>$ 28.000</Typography>
        </Box>
      </Box>
      <Box sx={{ borderRadius: 2, bgcolor: "secondary.main", p: 1 }}>
        <Typography variant="caption" sx={{ color: "#fff" }}>Meta viaje · 65%</Typography>
      </Box>
    </Box>
  );
}

export default function BienvenidaPage() {
  const router = useRouter();
  const go = (h: string) => router.push(h);

  return (
    <Box>
      {/* HERO */}
      <Box sx={{ bgcolor: "#12211B", color: "#F4F6F2", py: { xs: 6, md: 10 } }}>
        <Container maxWidth="md">
          <Box sx={{ display: "flex", flexDirection: { xs: "column", md: "row" }, gap: 4, alignItems: "center" }}>
            <Box sx={{ flex: 1 }}>
              <Typography variant="h6" sx={{ fontFamily: '"Sora", sans-serif', fontWeight: 700, mb: 2 }}>Finova</Typography>
              <Typography variant="h4" component="h1" sx={{ fontFamily: '"Sora", sans-serif', fontWeight: 700, mb: 2 }}>
                Sabé en qué se va tu plata, sin planillas
              </Typography>
              <Typography variant="body1" sx={{ mb: 3, opacity: 0.85 }}>
                Registrá ingresos y gastos en segundos, mirá gráficos de tu mes y preguntale a tu asistente financiero. Gratis y en español.
              </Typography>
              <Box sx={{ display: "flex", gap: 2, flexWrap: "wrap" }}>
                <Button variant="contained" color="primary" size="large" endIcon={<FiArrowRight />} onClick={() => go("/registro")}>
                  Crear cuenta gratis
                </Button>
                <Button variant="text" sx={{ color: "#F4F6F2" }} onClick={() => go("/login")}>
                  Ya tengo cuenta
                </Button>
              </Box>
            </Box>
            <PhoneMockup />
          </Box>
        </Container>
      </Box>

      {/* PROBLEMA */}
      <Container maxWidth="md" sx={{ py: 6 }}>
        <Typography variant="h5" sx={{ fontFamily: '"Sora", sans-serif', fontWeight: 700, mb: 2 }}>
          Llega fin de mes y no sabés dónde quedó tu sueldo
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Las planillas se abandonan a la segunda semana. Finova te pide 10 segundos por gasto y a cambio te muestra tu realidad financiera completa.
        </Typography>
      </Container>

      {/* FUNCIONES */}
      <Box sx={{ bgcolor: "background.paper", py: 6 }}>
        <Container maxWidth="md" sx={{ display: "flex", flexDirection: "column", gap: 3 }}>
          <Typography variant="h5" sx={{ fontFamily: '"Sora", sans-serif', fontWeight: 700 }}>Todo tu dinero en un lugar</Typography>
          <Feature icon={<FiCreditCard size={24} />} title="Registrá en segundos" text="Ingresos y gastos con cuenta, categoría y fecha. Sin fricción, desde el celular." />
          <Feature icon={<FiPieChart size={24} />} title="Mirá tu mes de un vistazo" text="Gráficos por categoría, comparativas con el mes anterior y evolución de tu saldo." />
          <Feature icon={<FiTarget size={24} />} title="Presupuestos y metas" text="Poné límites por categoría y metas de ahorro con aportes reales desde tus cuentas." />
          <Feature icon={<FiMessageCircle size={24} />} title="Asistente financiero" text="Preguntale cuánto gastaste, en qué más gastás o cómo ahorrar. Responde con tus datos reales." />
          <Feature icon={<FiShield size={24} />} title="Tus datos, solo tuyos" text="Cada usuario ve únicamente su información. Sin publicidad, sin venta de datos." />
        </Container>
      </Box>

      {/* CÓMO FUNCIONA */}
      <Container maxWidth="md" sx={{ py: 6, display: "flex", flexDirection: "column", gap: 2 }}>
        <Typography variant="h5" sx={{ fontFamily: '"Sora", sans-serif', fontWeight: 700 }}>Empezás en 3 pasos</Typography>
        <Typography variant="body1"><b>1. Creá tu cuenta</b> — solo email y contraseña.</Typography>
        <Typography variant="body1"><b>2. Cargá una cuenta</b> — efectivo, banco o billetera, con su moneda.</Typography>
        <Typography variant="body1"><b>3. Registrá tu primer gasto</b> — el panel, los gráficos y el asistente hacen el resto.</Typography>
      </Container>

      {/* FAQ */}
      <Box sx={{ bgcolor: "background.paper", py: 6 }}>
        <Container maxWidth="md" sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
          <Typography variant="h5" sx={{ fontFamily: '"Sora", sans-serif', fontWeight: 700 }}>Preguntas frecuentes</Typography>
          <Typography variant="body1"><b>¿Es gratis?</b> — Sí, Finova es gratis.</Typography>
          <Typography variant="body1"><b>¿Puedo usar varias monedas?</b> — Sí, cada cuenta tiene su moneda y los totales se agrupan por moneda.</Typography>
          <Typography variant="body1"><b>¿Mis datos están seguros?</b> — Cada usuario accede solo a sus datos y las claves nunca se guardan en texto plano.</Typography>
          <Typography variant="body1"><b>¿El asistente inventa respuestas?</b> — No, responde únicamente con los datos que registraste.</Typography>
          <Typography variant="body1"><b>¿Funciona en el celular?</b> — Sí, está diseñada primero para móvil.</Typography>
        </Container>
      </Box>

      {/* CTA FINAL */}
      <Box sx={{ bgcolor: "#12211B", color: "#F4F6F2", py: 6, textAlign: "center" }}>
        <Container maxWidth="sm">
          <Typography variant="h5" sx={{ fontFamily: '"Sora", sans-serif', fontWeight: 700, mb: 2 }}>
            Empezá hoy a entender tu plata
          </Typography>
          <Button variant="contained" color="primary" size="large" endIcon={<FiArrowRight />} onClick={() => go("/registro")}>
            Crear cuenta gratis
          </Button>
          <Typography variant="caption" sx={{ display: "block", mt: 2, opacity: 0.7 }}>
            <FiBarChart2 style={{ verticalAlign: "middle" }} /> Sin tarjeta, sin compromiso
          </Typography>
        </Container>
      </Box>
    </Box>
  );
}

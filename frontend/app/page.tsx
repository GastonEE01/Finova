"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { getToken } from "./lib/auth";
import BienvenidaPage from "./bienvenida/page";

export default function Home() {
  const router = useRouter();
  const [checked, setChecked] = useState(false);
  const [logged, setLogged] = useState(false);

  useEffect(() => {
    if (getToken()) {
      router.replace("/dashboard");
    } else {
      setLogged(false);
      setChecked(true);
    }
  }, [router]);

  if (!checked) return null;
  if (logged) return null;
  return <BienvenidaPage />;
}

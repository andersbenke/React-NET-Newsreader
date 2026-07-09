function CatNews({category}) {
  const [currentCategory, setCurrentCategory] = React.useState(null);

  React.useEffect(() => {
    const handleCustomEvent = (event) => {
      // The data lives inside event.detail
      setCurrentCategory(event.detail.message);
    };

    // Listen to the event on the window object
    window.addEventListener("nav-link-click", handleCustomEvent);

    // Clean up the listener when the component unmounts
    return () => {
      window.removeEventListener("nav-link-click", handleCustomEvent);
    };
  }, []);

  return (<section className="block" aria-labelledby="tech-title">
      <div className="wrap">
        <header>
          <h2 id="tech-title">{currentCategory ? currentCategory : "N/A"}</h2>
          <a href="#">/v2/top-headlines?category=technology</a>
        </header>
        <div className="cards">
          <article className="card">
            <p className="meta"><b>The Verge</b> · 07:15</p>
            <h3><a href="#">RISC-V laptops quietly reach price parity with mid-range x86</a></h3>
            <p>The open ISA's decade of embedded dominance finally spills onto the desk.</p>
          </article>
          <article className="card">
            <p className="meta"><b>SVT Nyheter</b> · 06:58</p>
            <h3><a href="#">Swedish municipalities pool budgets for shared open-source IT stack</a></h3>
            <p>Forty-one kommuner sign on; procurement lawyers reportedly thrilled and terrified.</p>
          </article>
          <article className="card">
            <p className="meta"><b>Ars Technica</b> · 05:30</p>
            <h3><a href="#">Terminal editors see third straight year of user growth</a></h3>
            <p>Researchers cite "IDE fatigue" and the unreasonable durability of muscle memory.</p>
          </article>
        </div>
      </div>
    </section>
  );
}
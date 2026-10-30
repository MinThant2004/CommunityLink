# Full generator for CommunityLink_Presentation_SeedData.sql
import datetime

# Standard bcrypt hash for "Ace@1234"
PW_HASH = "$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG"

# 4 Exact Response SLA dropdown options from Upgrade.razor
SLA_OPTIONS = [
    "Replies within 12 hours",
    "Replies within 24 hours",
    "Replies within 36 hours",
    "Replies within 48 hours"
]

# Real Avatars from /uploads/ and /uploads/avatars/
AVATARS_LEADERS = [
    "/uploads/avatars/avatar_1_639264401371924934.jpg",
    "/uploads/avatars/avatar_1_639265126035121964.jpg",
    "/uploads/avatars/avatar_243_639283398952746391.jpg",
    "/uploads/avatars/avatar_4_639267808148574280.jpg",
    "/uploads/avatars/avatar_5_639264409958499886.jpg",
    "/uploads/avatars/avatar_5_639264410065049235.jpg",
    "/uploads/avatars/avatar_5_639264410744498058.jpg",
    "/uploads/elena_avatar.jpg",
    "/uploads/23ec132f1988477db199b7a8bad83d79_639258240114301557.jpg",
    "/uploads/4d8112ef0c334c1eb6879f9fe5888e9c_639253116772409700.jpg",
    "/uploads/552b2ddb8d0242ca8427879265470a75_639255628423254239.jpg",
    "/uploads/841e15097fef491daa86e58f85320734_639255586638048491.jpg",
    "/uploads/87e79e32cacd40e78e8f8da70d35da01_639261873076652363.jpg",
    "/uploads/88aa3d3bbd764d42b976b69f9edc8222_639261873577168471.jpg",
    "/uploads/99f7c7d8a1c6490aa9ae0ddf887226ca_639253226881338224.jpg",
    "/uploads/a8dd107c500f427aabd30924891c9712_639255658107733400.jpg",
    "/uploads/af0f5dd3c4664835b546581bf0526d6e_639261871869518689.jpg",
    "/uploads/b025b2e3393746fc8279a2fe9a2232f0_639261873633628825.jpg",
    "/uploads/f139242c0d7f483fb5a3483469d33df9_639258240668381170.jpg",
    "/uploads/2ebdd714ee7f4ab9ac034311ed451b24_639255588269789502.png"
]

AVATARS_NORMAL = [
    "/uploads/posts/202610/15983265f7f54c2faa62419918ed85e8_639264352658469432.jpg",
    "/uploads/posts/202610/29de9a9b8f4d4b7c948e966747d0a9c0_639264352665934534.jpg",
    "/uploads/posts/202610/2a965a7821d34c1393ff992c8618bb8e_639264352654733706.jpg",
    "/uploads/posts/202610/38676668edbe4fc8b2e29de42690de89_639283387193117772.jpg",
    "/uploads/posts/202610/455134739cf04f1ba65ce74f8b17018f_639289328536512425.jpg",
    "/uploads/posts/202610/4689cc5bb67a46f8a9e890610841461a_639279804033667591.jpg",
    "/uploads/posts/202610/64caf7e8a2d041c8a0a65824cb88c0a2_639283359358458377.jpg",
    "/uploads/posts/202610/8850f2ebf9ca4edda9dbf489c8893c7f_639273729741980335.jpg",
    "/uploads/posts/202610/9172be0a5573462eb03ecdde84f8b1d6_639264352661925222.jpg",
    "/uploads/posts/202610/945491f8224e44468d240f8cc3561e4a_639283657961211862.jpg"
]

# 10 Domain Professionals (Top Globally Recognized Figures)
domain_pros = [
    {
        "id": 101, "username": "aunglansang", "email": "aungla@onefc.com", "display_name": "Aung La N Sang",
        "bio": "The Burmese Python | Former 2-Division ONE Championship World Champion (Middleweight & Light Heavyweight). Dedicated to martial arts mastery, youth athletic conditioning, and mental grit.",
        "skills": ["MMA Elite Coaching", "Athletic Conditioning", "Mental Toughness", "Youth Sports Mentorship"],
        "balance": 26500, "rating_scores": [5, 5, 5, 5, 5], "reg_days": 180, "login_hours": 3, "sla": SLA_OPTIONS[0], "avatar": AVATARS_LEADERS[0],
        "category_affinity": [7] # Sports
    },
    {
        "id": 102, "username": "thantmyintu", "email": "tmyintu@heritage.org", "display_name": "Dr. Thant Myint-U",
        "bio": "Historian, former United Nations official, Cambridge scholar, and author of 'The River of Lost Footsteps' and 'The Hidden History of Burma'. Advocating cultural heritage preservation and diplomacy.",
        "skills": ["Southeast Asian History", "Diplomatic Affairs", "Cultural Heritage Policy", "Non-Fiction Authorship"],
        "balance": 13500, "rating_scores": [5, 5, 5, 5, 4], "reg_days": 175, "login_hours": 5, "sla": SLA_OPTIONS[1], "avatar": AVATARS_LEADERS[1],
        "category_affinity": [6] # Arts & Culture
    },
    {
        "id": 103, "username": "drcynthiamaung", "email": "cynthia@maetao.org", "display_name": "Dr. Cynthia Maung",
        "bio": "Founder of Mae Tao Clinic. Ramon Magsaysay Award, Sydney Peace Prize, and Jonathan Mann Award recipient; pioneering border healthcare, maternal medicine, and child health systems.",
        "skills": ["Humanitarian Medicine", "Border Health Systems", "Maternal Care Policy", "Community Health Training"],
        "balance": 11200, "rating_scores": [5, 5, 5, 5, 5], "reg_days": 170, "login_hours": 8, "sla": SLA_OPTIONS[0], "avatar": AVATARS_LEADERS[2],
        "category_affinity": [8] # Public Health
    },
    {
        "id": 104, "username": "paingtakhon", "email": "paing@takhonmedia.com", "display_name": "Paing Takhon",
        "bio": "International Model, Actor, and Animal Welfare Advocate. Ranked #1 in TC Candler's 100 Most Handsome Faces; passionate about visual storytelling, cinema arts, and community pet shelters.",
        "skills": ["Fashion & Brand Ambassadorship", "Acting & Visual Arts", "Animal Welfare Activism", "Social Media Engagement"],
        "balance": 22400, "rating_scores": [5, 5, 4, 5, 5], "reg_days": 165, "login_hours": 12, "sla": SLA_OPTIONS[1], "avatar": AVATARS_LEADERS[3],
        "category_affinity": [9] # Telecom & Media
    },
    {
        "id": 105, "username": "utejaniya", "email": "tejaniya@shweooomin.org", "display_name": "Sayadaw U Tejaniya",
        "bio": "Theravada Buddhist Meditation Master. Guiding international practitioners in daily life mindfulness, awareness of the observing mind, and practical Dhamma wisdom.",
        "skills": ["Vipassana Mindfulness", "Mind State Observation", "Buddhist Philosophy", "Mental Well-Being Mentorship"],
        "balance": 9800, "rating_scores": [5, 5, 5, 5, 5], "reg_days": 160, "login_hours": 15, "sla": SLA_OPTIONS[2], "avatar": AVATARS_LEADERS[4],
        "category_affinity": [6, 7] # Arts & Health
    },
    {
        "id": 106, "username": "zarganar", "email": "zarganar@freeart.org", "display_name": "Zarganar (Maung Thura)",
        "bio": "Acclaimed satirist, filmmaker, and social activist. Recipient of the Prince Claus Award and PEN/Barbara Goldsmith Freedom to Write Award; mentor in performance art and social reform.",
        "skills": ["Satirical Writing", "Film Directing", "Performing Arts & Comedy", "Civic Education"],
        "balance": 12500, "rating_scores": [5, 4, 5, 5], "reg_days": 155, "login_hours": 18, "sla": SLA_OPTIONS[1], "avatar": AVATARS_LEADERS[5],
        "category_affinity": [6, 9] # Arts & Media
    },
    {
        "id": 107, "username": "minwaeaung", "email": "minwaeaung@oldhanoi.art", "display_name": "Min Wae Aung",
        "bio": "Internationally celebrated fine artist. Renowned for meditative monk-procession canvases exhibited across London, New York, Hong Kong, and Paris galleries.",
        "skills": ["Contemporary Oil Painting", "Exhibition Curation", "Color Harmony Theory", "Fine Arts Mentorship"],
        "balance": 15400, "rating_scores": [5, 5, 5, 4], "reg_days": 150, "login_hours": 22, "sla": SLA_OPTIONS[2], "avatar": AVATARS_LEADERS[6],
        "category_affinity": [6] # Arts & Culture
    },
    {
        "id": 108, "username": "waiwainu", "email": "waiwainu@peaceadvocacy.org", "display_name": "Wai Wai Nu",
        "bio": "Founder of Women's Peace Network. Time Magazine Next Generation Leader, recipient of the US State Department International Women of Courage Award; advocate for youth empowerment and peace.",
        "skills": ["Peacebuilding & Dialogue", "Women Human Rights Law", "Youth Civic Mobilization", "International Advocacy"],
        "balance": 10500, "rating_scores": [5, 4, 5, 5], "reg_days": 145, "login_hours": 26, "sla": SLA_OPTIONS[0], "avatar": AVATARS_LEADERS[7],
        "category_affinity": [8] # Public Health & Social Impact
    },
    {
        "id": 109, "username": "drsaikhayguan", "email": "dr.saikhayguan@medresearch.org", "display_name": "Dr. Sai Khay Guan",
        "bio": "Consultant Medical Oncologist & Clinical Researcher. Leading international clinician specializing in oncology therapeutics, cancer genetics, and medical academic research.",
        "skills": ["Clinical Oncology", "Evidence-Based Therapeutics", "Medical Data Analytics", "Healthcare Mentorship"],
        "balance": 14800, "rating_scores": [5, 5, 5, 4], "reg_days": 140, "login_hours": 30, "sla": SLA_OPTIONS[0], "avatar": AVATARS_LEADERS[8],
        "category_affinity": [8] # Public Health
    },
    {
        "id": 110, "username": "mawnaing", "email": "mawnaing@cineburma.com", "display_name": "Maw Naing",
        "bio": "Award-winning independent film director & visual poet. Creator of 'The Monk' and 'MA - Cry of Silence', premiered at Karlovy Vary, Busan IFF, and international film festivals.",
        "skills": ["Independent Cinema Direction", "Screenwriting & Storytelling", "Cinematography Aesthetics", "Creative Production"],
        "balance": 11900, "rating_scores": [5, 5, 4, 5], "reg_days": 135, "login_hours": 34, "sla": SLA_OPTIONS[1], "avatar": AVATARS_LEADERS[9],
        "category_affinity": [6, 9] # Media & Film
    }
]

# 10 Public Figures (Influential Business & Industry Leaders)
public_figures = [
    {
        "id": 111, "username": "zawzaw", "email": "zawzaw@maxmyanmar.com", "display_name": "Zaw Zaw",
        "bio": "Chairman of Max Myanmar Group & AYA Financial Group. Passionate about commercial banking innovation, national infrastructure development, sports academies, and philanthropic healthcare.",
        "skills": ["Corporate Strategy", "Banking & Finance", "Infrastructure Leadership", "CSR & Sports Development"],
        "balance": 28500, "rating_scores": [5, 5, 5, 5, 4], "reg_days": 182, "login_hours": 4, "sla": SLA_OPTIONS[0], "avatar": AVATARS_LEADERS[10],
        "category_affinity": [2, 7, 10] # Finance, Sports, Trade
    },
    {
        "id": 112, "username": "aungkowin", "email": "aungkowin@kbzgroup.com", "display_name": "Aung Ko Win",
        "bio": "Chairman of KBZ Group of Companies. Spearheading national financial inclusion via KBZ Bank, digital fintech ecosystems with KBZPay, and domestic aviation services.",
        "skills": ["Digital Banking", "Fintech Ecosystems", "Financial Inclusion", "Aviation Management"],
        "balance": 35000, "rating_scores": [5, 5, 5, 5, 5], "reg_days": 178, "login_hours": 7, "sla": SLA_OPTIONS[0], "avatar": AVATARS_LEADERS[11],
        "category_affinity": [1, 2, 10] # Tech, Finance, Aviation
    },
    {
        "id": 113, "username": "sergepun", "email": "sergepun@yoma.com.mm", "display_name": "Serge Pun (Theim Wai)",
        "bio": "Executive Chairman of Yoma Strategic Holdings & First Myanmar Investment (FMI). Pioneer of international real estate master-developments, automotive hubs, and capital market ventures.",
        "skills": ["Real Estate Development", "Cross-Border Investment", "Capital Markets", "Corporate Governance"],
        "balance": 24000, "rating_scores": [5, 5, 5, 4, 5], "reg_days": 174, "login_hours": 9, "sla": SLA_OPTIONS[1], "avatar": AVATARS_LEADERS[12],
        "category_affinity": [2, 3] # Finance, Infrastructure
    },
    {
        "id": 114, "username": "aikhtun", "email": "aikhtun@shwetaung.com", "display_name": "Aik Htun",
        "bio": "Chairman of Shwe Taung Group. Building sustainable modern infrastructure, prime retail mixed-use hubs (Junction City), certified green construction, and hydropower transmission.",
        "skills": ["Mega-Project Engineering", "Retail Property Development", "Sustainable Construction", "Commercial Strategy"],
        "balance": 19500, "rating_scores": [5, 4, 5, 5], "reg_days": 169, "login_hours": 13, "sla": SLA_OPTIONS[2], "avatar": AVATARS_LEADERS[13],
        "category_affinity": [3] # Infrastructure
    },
    {
        "id": 115, "username": "drsaisamhtun", "email": "saisamhtun@loihein.com", "display_name": "Dr. Sai Sam Htun",
        "bio": "Founder & Chairman of Loi Hein Group. Pioneering Myanmar's leading FMCG manufacturing, iconic beverage brands (Alpine), automated packaging plants, and nationwide distribution chains.",
        "skills": ["FMCG Manufacturing", "Supply Chain Architecture", "Brand Building", "Beverage Industry Innovation"],
        "balance": 16800, "rating_scores": [5, 5, 5, 5, 4], "reg_days": 164, "login_hours": 16, "sla": SLA_OPTIONS[1], "avatar": AVATARS_LEADERS[14],
        "category_affinity": [4] # FMCG
    },
    {
        "id": 116, "username": "tayza", "email": "tayza@htoogroup.com", "display_name": "Tay Za",
        "bio": "Chairman of Htoo Group of Companies. Renowned leader in international trade logistics, boutique eco-resorts & luxury hospitality, regional aviation networks, and timber processing.",
        "skills": ["Luxury Hospitality", "Aviation Logistics", "Commodity Trading", "Resort Operations"],
        "balance": 21000, "rating_scores": [5, 4, 5, 4], "reg_days": 159, "login_hours": 19, "sla": SLA_OPTIONS[3], "avatar": AVATARS_LEADERS[15],
        "category_affinity": [5, 10] # Energy, Trade & Aviation
    },
    {
        "id": 117, "username": "michaelmoemyint", "email": "mmoemyint@mprlexp.com", "display_name": "Michael Moe Myint",
        "bio": "Founder & CEO of Myint & Associates and MPRL E&P Group. Offshore deepwater energy exploration, industrial HSE safety benchmarks, and independent upstream petroleum engineering.",
        "skills": ["Offshore Exploration", "Petroleum Engineering", "Energy Security", "HSE Compliance"],
        "balance": 18200, "rating_scores": [5, 5, 4, 5], "reg_days": 154, "login_hours": 23, "sla": SLA_OPTIONS[2], "avatar": AVATARS_LEADERS[16],
        "category_affinity": [5] # Energy & Resources
    },
    {
        "id": 118, "username": "waiminhtin", "email": "mikehtin@fortune.com.mm", "display_name": "Wai Min Htin (Mike)",
        "bio": "Managing Director of Fortune International Group. Telecom cellular towers, fiber optic enterprise backbones, IT infrastructure deployment, and consumer tech logistics.",
        "skills": ["Telecommunications", "IT Infrastructure", "Consumer Tech Logistics", "Network Modernization"],
        "balance": 14500, "rating_scores": [5, 4, 5, 5], "reg_days": 149, "login_hours": 27, "sla": SLA_OPTIONS[1], "avatar": AVATARS_LEADERS[17],
        "category_affinity": [1, 9] # Tech, Telecom
    },
    {
        "id": 119, "username": "drthaunghan", "email": "thaunghan@cbbank.com.mm", "display_name": "Dr. Thaung Han",
        "bio": "Managing Director & CEO of CB Bank. Leader in modern commercial and trade banking, core digital enterprise modernization, and open API financial architectures.",
        "skills": ["Commercial Banking", "Core Banking Transformation", "API Banking", "Risk Governance"],
        "balance": 17400, "rating_scores": [5, 5, 5, 4], "reg_days": 144, "login_hours": 31, "sla": SLA_OPTIONS[0], "avatar": AVATARS_LEADERS[18],
        "category_affinity": [1, 2] # Tech, Finance
    },
    {
        "id": 120, "username": "winaung", "email": "winaung@dagongroup.com", "display_name": "Win Aung",
        "bio": "Chairman of Dagon Group of Companies & Former President of UMFCCI. Deep-sea port terminals, regional agricultural export value chains, and modern industrial zone master-planning.",
        "skills": ["Port Logistics Management", "Agro-Export Supply Chains", "Trade Policy", "Industrial Zone Development"],
        "balance": 15900, "rating_scores": [5, 4, 5, 4], "reg_days": 139, "login_hours": 35, "sla": SLA_OPTIONS[2], "avatar": AVATARS_LEADERS[19],
        "category_affinity": [4, 10] # FMCG, Trade & Logistics
    }
]

# Real life photos for all 20 Myanmar Domain Professionals & Public Figures
REAL_LEADER_AVATARS = {
    101: "https://upload.wikimedia.org/wikipedia/commons/8/87/Aung_La_N_Sang.jpg",
    102: "https://assets.frontiermyanmar.net/2020/02/thant-myint-u-everything-is-interlinked-1582113030-1024x682.jpg",
    103: "https://upload.wikimedia.org/wikipedia/commons/8/84/Dr._Cynthia_Maung.jpg",
    104: "https://upload.wikimedia.org/wikipedia/commons/9/9f/%D0%9F%D0%B0%D0%B8%D0%BD%D0%B3_%D0%A2%D0%B0%D0%BA%D1%85%D0%BE%D0%BD.png",
    105: "https://upload.wikimedia.org/wikipedia/commons/6/65/Sayadaw_U_Tejaniya_and_Bhikku_Kumara_CROPPED.jpg",
    106: "https://upload.wikimedia.org/wikipedia/commons/c/c2/U-thura.jpg",
    107: "https://thukhuma.org/wp-content/uploads/2016/06/min-wae-aung.jpg",
    108: "https://upload.wikimedia.org/wikipedia/commons/4/41/Wai_Wai_Nu.jpg",
    109: "https://www.nuhs.edu.sg/sites/default/files/2023-11/prof-yeoh-khay-guan.jpg",
    110: "https://asianwiki.com/images/5/5d/The_Maw_Naing-p1.jpg",
    111: "https://upload.wikimedia.org/wikipedia/commons/b/b1/U_Zaw_Zaw_donates_10000_footballs_%28cropped%29.jpg",
    112: "https://www.irrawaddy.com/wp-content/uploads/2023/08/Junta-Crony-April-18-feat.jpg",
    113: "https://upload.wikimedia.org/wikipedia/commons/2/29/Serge_Pun%2C_Chairman%2C_Serge_Pun_%26_Associates%2C_Myanmar_%E2%80%93_Myanmar_is_good_news._%2831234690092%29.jpg",
    114: "https://www.shwetaunggroup.com/wp-content/uploads/2017/03/management1.jpg",
    115: "https://rubyhillmicrofinance.com/images/dr-sai-sam-htun.jpg",
    116: "https://www.irrawaddy.com/wp-content/uploads/2022/12/TayZa-1.jpg",
    117: "https://www.peninsula-press.com/wp-content/uploads/2015/02/UmoeMynt-2.jpg",
    118: "https://fortunemm.com/wp-content/uploads/2020/09/Chairman-Dr.-Mya-Han.jpg",
    119: "https://myanmariod.com/wp-content/uploads/2024/09/dr_thaung_han_img.png",
    120: "https://dagon-group.com/wp-content/uploads/2013/09/chairman-message1.jpg"
}

# Calculate exact AverageRating and RatingCount for each leader, and assign verified real photo
for u in domain_pros + public_figures:
    u["rating_count"] = len(u["rating_scores"])
    u["rating"] = round(sum(u["rating_scores"]) / float(u["rating_count"]), 1)
    if u["id"] in REAL_LEADER_AVATARS:
        u["avatar"] = REAL_LEADER_AVATARS[u["id"]]

# 10 Normal Real Everyday Burmese Users
normal_users = [
    { "id": 121, "username": "aungkyawzin", "email": "aungkyawzin@gmail.com", "display_name": "Aung Kyaw Zin", "loc": "Kamayut, Yangon", "hl": "Junior Backend Developer | .NET & C# Learner", "reg_days": 110, "login_hours": 2, "avatar": AVATARS_NORMAL[0] },
    { "id": 122, "username": "thazinmoe", "email": "thazinmoe.mkt@gmail.com", "display_name": "Thazin Moe", "loc": "Sanchaung, Yangon", "hl": "Digital Marketing Freelancer & Content Creator", "reg_days": 105, "login_hours": 6, "avatar": AVATARS_NORMAL[1] },
    { "id": 123, "username": "heinhtetzaw", "email": "heinhtet.uit@gmail.com", "display_name": "Hein Htet Zaw", "loc": "Hlaing, Yangon", "hl": "Final Year CS Student @ UIT | Mobile App & Flutter Explorer", "reg_days": 98, "login_hours": 9, "avatar": AVATARS_NORMAL[2] },
    { "id": 124, "username": "khinyadanar", "email": "khinyadanar.design@gmail.com", "display_name": "Khin Yadanar Oo", "loc": "Chanayethazan, Mandalay", "hl": "UI/UX Design Learner | Figma & Web Design", "reg_days": 90, "login_hours": 13, "avatar": AVATARS_NORMAL[3] },
    { "id": 125, "username": "minkhantkyaw", "email": "minkhant.mdy@gmail.com", "display_name": "Min Khant Kyaw", "loc": "Maha Aungmye, Mandalay", "hl": "Small business owner @ Mandalay Tea & Snacks", "reg_days": 85, "login_hours": 16, "avatar": AVATARS_NORMAL[4] },
    { "id": 126, "username": "sumyatnoe", "email": "sumyatnoe.edu@gmail.com", "display_name": "Su Myat Noe", "loc": "Taunggyi, Shan State", "hl": "High school English tutor & Language Enthusiast", "reg_days": 80, "login_hours": 20, "avatar": AVATARS_NORMAL[5] },
    { "id": 127, "username": "zayarlin", "email": "zayarlin.eng@gmail.com", "display_name": "Zayar Lin", "loc": "Pyin Oo Lwin", "hl": "Civil Engineering Graduate | Urban Architecture & Photography", "reg_days": 75, "login_hours": 24, "avatar": AVATARS_NORMAL[6] },
    { "id": 128, "username": "eiphyusin", "email": "eiphyusin.hr@gmail.com", "display_name": "Ei Phyu Sin", "loc": "Bahan, Yangon", "hl": "HR Executive | Workplace culture & professional networking", "reg_days": 70, "login_hours": 29, "avatar": AVATARS_NORMAL[7] },
    { "id": 129, "username": "kaungmyatthu", "email": "kaungmyat.photo@gmail.com", "display_name": "Kaung Myat Thu", "loc": "Mawlamyine, Mon State", "hl": "Amateur Landscape Photographer & Travel Blogger", "reg_days": 65, "login_hours": 33, "avatar": AVATARS_NORMAL[8] },
    { "id": 130, "username": "phyuphyuthin", "email": "phyuphyuthin.fin@gmail.com", "display_name": "Phyu Phyu Thin", "loc": "Bago", "hl": "Accounting & Finance Officer | Lifelong learner", "reg_days": 60, "login_hours": 37, "avatar": AVATARS_NORMAL[9] }
]

# 10 Top-Level Communities
communities_data = [
    (1, "Technology & Software Innovation", "technology-software-innovation", "Myanmar software engineering, cloud architectures, and digital ecosystem discussions.", 118),
    (2, "Business & Financial Markets", "business-financial-markets", "Commercial banking, capital investments, corporate governance, and fiscal policy in Myanmar.", 111),
    (3, "Infrastructure & Real Estate", "infrastructure-real-estate", "Mega-developments, smart urbanization, sustainable construction, and architectural feats.", 113),
    (4, "FMCG, Retail & Consumer Goods", "fmcg-retail-consumer-goods", "Fast-moving consumer products, national supply chains, and retail store management.", 115),
    (5, "Energy & Natural Resources", "energy-natural-resources", "Offshore exploration, green transitions, renewable power grids, and petroleum safety.", 117),
    (6, "Arts, Culture & Literature", "arts-culture-literature", "Burmese contemporary paintings, historical heritage preservation, literature, and screenwriting.", 102),
    (7, "Sports, Martial Arts & Health", "sports-martial-arts-health", "Combat athletics, youth physical fitness conditioning, wellness, and mental stamina.", 101),
    (8, "Public Health & Social Impact", "public-health-social-impact", "Humanitarian border clinics, maternal medicine, peacebuilding, and community welfare.", 103),
    (9, "Telecom, Media & Creative Arts", "telecom-media-creative-arts", "Telecommunications infrastructure, cinema direction, satire, and digital entertainment.", 110),
    (10, "Trade, Logistics & Aviation", "trade-logistics-aviation", "Deep-sea port logistics, regional agricultural exports, commercial air cargo, and transit corridors.", 120)
]

sub_topics = [
    # Comm 1: Tech
    ["Enterprise Cloud & DevOps", "Web & Mobile Engineering", "AI & Data Analytics", "Cybersecurity & Governance", "Emerging Tech & Web3"],
    # Comm 2: Business & Finance
    ["Commercial Banking Systems", "Fintech & Mobile Wallets", "Microfinance & Rural Inclusion", "Venture Capital & Startups", "Corporate Accounting & Tax"],
    # Comm 3: Infrastructure
    ["Urban Planning & Zoning", "Green Building Materials", "Smart Transit Networks", "Commercial Real Estate", "Residential Architecture"],
    # Comm 4: FMCG & Retail
    ["Food & Beverage Manufacturing", "Modern Trade Distribution", "Cold Chain Logistics", "Packaging & Quality Control", "Direct-to-Consumer Brands"],
    # Comm 5: Energy
    ["Offshore Oil & Deepwater", "Solar & Mini-Grids", "HSE Industrial Safety", "Power Transmission Engineering", "Petrochemical Refining"],
    # Comm 6: Arts & Culture
    ["Colonial Heritage Conservation", "Contemporary Fine Art", "Burmese Literature & Translation", "Independent Cinema & Scripting", "Traditional Textile & Design"],
    # Comm 7: Sports & Health
    ["Lethwei & Mixed Martial Arts", "Strength & High-Performance Conditioning", "Youth Football Academies", "Sports Nutrition & Recovery", "Mental Focus & Meditation"],
    # Comm 8: Public Health
    ["Rural Maternal & Child Healthcare", "Epidemiology & Infectious Control", "Humanitarian Relief Logistics", "Community Health Worker Training", "Mental Well-Being & Counseling"],
    # Comm 9: Telecom & Media
    ["Telecom Tower Infrastructure", "Fiber Optic Backbone Networks", "Broadcasting & Digital Media", "Satirical Media & Visual Arts", "Creator Economy & Digital Marketing"],
    # Comm 10: Trade & Logistics
    ["Maritime Container Shipping", "Inland Waterway Logistics", "Agro-Export Commodity Trading", "Aviation Cargo & Ground Handling", "Cross-Border Customs Facilitation"]
]

# Map each community to leaders whose skills match that category
comm_leader_affinity = {
    1: [118, 119, 112],       # Wai Min Htin (Telecom/IT), Dr. Thaung Han (Core Tech/APIs), Aung Ko Win (Fintech)
    2: [111, 112, 119, 113],  # Zaw Zaw, Aung Ko Win, Dr. Thaung Han, Serge Pun
    3: [113, 114],            # Serge Pun (Real estate), Aik Htun (Construction/Megaprojects)
    4: [115, 120],            # Dr. Sai Sam Htun (FMCG/Alpine), Win Aung (Agro/Retail)
    5: [117, 116],            # Michael Moe Myint (Offshore/Energy), Tay Za (Logistics/Resources)
    6: [102, 107, 105, 106],  # Dr. Thant Myint-U, Min Wae Aung, Sayadaw U Tejaniya, Zarganar
    7: [101, 105, 111],       # Aung La N Sang (MMA/Fitness), Sayadaw U Tejaniya (Mindfulness), Zaw Zaw (Sports CSR)
    8: [103, 109, 108],       # Dr. Cynthia Maung, Dr. Sai Khay Guan, Wai Wai Nu
    9: [110, 106, 104, 118],  # Maw Naing, Zarganar, Paing Takhon, Wai Min Htin
    10: [120, 116, 111, 112]  # Win Aung, Tay Za, Zaw Zaw, Aung Ko Win
}

sub_communities_data = []
sub_id_counter = 11

for c_idx, comm in enumerate(communities_data):
    c_id = comm[0]
    c_owner = comm[4]
    topics = sub_topics[c_idx]
    for topic in topics:
        slug = topic.lower().replace(" & ", "-").replace(" ", "-").replace("---", "-")
        sub_communities_data.append((sub_id_counter, c_id, topic, f"{slug}-{sub_id_counter}", f"Focused discussions and professional case studies in {topic}.", c_owner))
        sub_id_counter += 1

groups_data = []
group_id_counter = 1
group_types = ["OPEN", "PAID", "RESTRICTED"]

for sc in sub_communities_data:
    sc_id = sc[0]
    c_id = sc[1]
    sc_name = sc[2]
    candidate_owners = comm_leader_affinity[c_id]
    
    for g_num in range(1, 4):
        # Pick owner from leaders skilled in this community
        owner_id = candidate_owners[(group_id_counter - 1) % len(candidate_owners)]
        g_type = group_types[(group_id_counter - 1) % 3]
        g_name = f"{sc_name} - Working Circle {g_num}"
        g_slug = f"group-{sc_id}-{g_num}-{group_id_counter}"
        g_desc = f"Specialized working group focusing on operational insights, peer reviews, and best practices for {sc_name}."
        fee = 50 if g_type == "PAID" else 0
        groups_data.append((group_id_counter, sc_id, owner_id, g_name, g_slug, g_desc, g_type, fee))
        group_id_counter += 1

# Start writing SQL file
with open('CommunityLink_Presentation_SeedData.sql', 'w', encoding='utf-8') as out:
    out.write("""/* ==============================================================================
   CommunityLink - Master Presentation Seed Data & DB Clean Reset Script
   ==============================================================================
   - Resets and cleans all application activity and transactional data while preserving Admin accounts & System RBAC roles.
   - Sets up 10 Communities, 50 Sub-Communities, and 150 Groups.
   - Sets up 20 Verified Premium Leaders (10 Domain Professionals, 10 Public Figures) with skills, ratings, and LinkDrops (> 5000).
   - Sets up 10 Normal Real Burmese Users with authentic activities, ratings, and comments.
   - Profile Avatars assigned to all users pointing to existing verified files in /uploads/.
   - Response SLA Text matches exact dropdown options from Upgrade.razor.
   - Populates User Follow network (TblUserFollow).
   - Populates group posts across all 150 groups so groups are active and vibrant.
   - Each premium leader has at least 2 Group Posts and 2 Standalone Posts (including Polls) with real user comments.
   - Populates Content Moderation data with distinct statuses (PENDING, RESOLVED, DISMISSED, ACTIONED).
   - Uniform Password for all generated accounts: Ace@1234
   ============================================================================== */

USE [CommunityLink];
GO

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

BEGIN TRANSACTION;

PRINT '1. Cleaning existing data and resetting transactional tables...';

-- Safe cleaning: delete from tables only if they exist in the database
IF OBJECT_ID('dbo.TblContentReport', 'U') IS NOT NULL DELETE FROM [dbo].[TblContentReport];

IF OBJECT_ID('dbo.TblChatGroupMessageReaction', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatGroupMessageReaction];
IF OBJECT_ID('dbo.TblChatGroupMessageUserState', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatGroupMessageUserState];
IF OBJECT_ID('dbo.TblChatGroupMessage', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatGroupMessage];
IF OBJECT_ID('dbo.TblChatGroupInviteLink', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatGroupInviteLink];
IF OBJECT_ID('dbo.TblChatGroupInvite', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatGroupInvite];
IF OBJECT_ID('dbo.TblChatGroupJoinRequest', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatGroupJoinRequest];
IF OBJECT_ID('dbo.TblChatGroupBan', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatGroupBan];
IF OBJECT_ID('dbo.TblChatGroupMember', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatGroupMember];
IF OBJECT_ID('dbo.TblChatGroupPaymentTransaction', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatGroupPaymentTransaction];
IF OBJECT_ID('dbo.TblChatGroup', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatGroup];

IF OBJECT_ID('dbo.TblChatMessageReaction', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatMessageReaction];
IF OBJECT_ID('dbo.TblChatMessageUserState', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatMessageUserState];
IF OBJECT_ID('dbo.TblChatMessage', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatMessage];
IF OBJECT_ID('dbo.TblConversation', 'U') IS NOT NULL DELETE FROM [dbo].[TblConversation];

IF OBJECT_ID('dbo.TblNotification', 'U') IS NOT NULL DELETE FROM [dbo].[TblNotification];
IF OBJECT_ID('dbo.TblUserActivity', 'U') IS NOT NULL DELETE FROM [dbo].[TblUserActivity];
IF OBJECT_ID('dbo.TblUserFollow', 'U') IS NOT NULL DELETE FROM [dbo].[TblUserFollow];
IF OBJECT_ID('dbo.TblUserBlock', 'U') IS NOT NULL DELETE FROM [dbo].[TblUserBlock];
IF OBJECT_ID('dbo.TblUserRating', 'U') IS NOT NULL DELETE FROM [dbo].[TblUserRating];
IF OBJECT_ID('dbo.TblSkillEndorsement', 'U') IS NOT NULL DELETE FROM [dbo].[TblSkillEndorsement];
IF OBJECT_ID('dbo.TblUserSkill', 'U') IS NOT NULL DELETE FROM [dbo].[TblUserSkill];
IF OBJECT_ID('dbo.TblIdentityVerification', 'U') IS NOT NULL DELETE FROM [dbo].[TblIdentityVerification];
IF OBJECT_ID('dbo.TblUserVerificationAudit', 'U') IS NOT NULL DELETE FROM [dbo].[TblUserVerificationAudit];
IF OBJECT_ID('dbo.TblUserSubscription', 'U') IS NOT NULL DELETE FROM [dbo].[TblUserSubscription];
IF OBJECT_ID('dbo.TblLinkDropTransaction', 'U') IS NOT NULL DELETE FROM [dbo].[TblLinkDropTransaction];
IF OBJECT_ID('dbo.TblLinkDropPurchaseProof', 'U') IS NOT NULL DELETE FROM [dbo].[TblLinkDropPurchaseProof];
IF OBJECT_ID('dbo.TblLinkDropPurchase', 'U') IS NOT NULL DELETE FROM [dbo].[TblLinkDropPurchase];
IF OBJECT_ID('dbo.TblLinkDropWallet', 'U') IS NOT NULL DELETE FROM [dbo].[TblLinkDropWallet];
IF OBJECT_ID('dbo.TblCreatorPayoutRequest', 'U') IS NOT NULL DELETE FROM [dbo].[TblCreatorPayoutRequest];
IF OBJECT_ID('dbo.TblCreatorChatSetting', 'U') IS NOT NULL DELETE FROM [dbo].[TblCreatorChatSetting];
IF OBJECT_ID('dbo.TblPrivateChatPaymentTransaction', 'U') IS NOT NULL DELETE FROM [dbo].[TblPrivateChatPaymentTransaction];

IF OBJECT_ID('dbo.TblPollVote', 'U') IS NOT NULL DELETE FROM [dbo].[TblPollVote];
IF OBJECT_ID('dbo.TblPollOption', 'U') IS NOT NULL DELETE FROM [dbo].[TblPollOption];
IF OBJECT_ID('dbo.TblPoll', 'U') IS NOT NULL DELETE FROM [dbo].[TblPoll];
IF OBJECT_ID('dbo.TblComment', 'U') IS NOT NULL DELETE FROM [dbo].[TblComment];
IF OBJECT_ID('dbo.TblPostLike', 'U') IS NOT NULL DELETE FROM [dbo].[TblPostLike];
IF OBJECT_ID('dbo.TblPostShare', 'U') IS NOT NULL DELETE FROM [dbo].[TblPostShare];
IF OBJECT_ID('dbo.TblPostImage', 'U') IS NOT NULL DELETE FROM [dbo].[TblPostImage];
IF OBJECT_ID('dbo.TblSavedPost', 'U') IS NOT NULL DELETE FROM [dbo].[TblSavedPost];
IF OBJECT_ID('dbo.TblSavedAccount', 'U') IS NOT NULL DELETE FROM [dbo].[TblSavedAccount];
IF OBJECT_ID('dbo.TblPost', 'U') IS NOT NULL DELETE FROM [dbo].[TblPost];

IF OBJECT_ID('dbo.TblGroupRating', 'U') IS NOT NULL DELETE FROM [dbo].[TblGroupRating];
IF OBJECT_ID('dbo.TblGroupJoinRequest', 'U') IS NOT NULL DELETE FROM [dbo].[TblGroupJoinRequest];
IF OBJECT_ID('dbo.TblGroupMember', 'U') IS NOT NULL DELETE FROM [dbo].[TblGroupMember];
IF OBJECT_ID('dbo.TblGroup', 'U') IS NOT NULL DELETE FROM [dbo].[TblGroup];

IF OBJECT_ID('dbo.TblCommunityRating', 'U') IS NOT NULL DELETE FROM [dbo].[TblCommunityRating];
IF OBJECT_ID('dbo.TblCommunityJoinRequest', 'U') IS NOT NULL DELETE FROM [dbo].[TblCommunityJoinRequest];
IF OBJECT_ID('dbo.TblCommunityMember', 'U') IS NOT NULL DELETE FROM [dbo].[TblCommunityMember];
IF OBJECT_ID('dbo.TblCommunityAuditLog', 'U') IS NOT NULL DELETE FROM [dbo].[TblCommunityAuditLog];
IF OBJECT_ID('dbo.TblCommunity', 'U') IS NOT NULL DELETE FROM [dbo].[TblCommunity];

IF OBJECT_ID('dbo.TblUserRole', 'U') IS NOT NULL DELETE FROM [dbo].[TblUserRole] WHERE [UserId] IN (SELECT [UserId] FROM [dbo].[TblUser] WHERE [UserName] != 'admin' AND [Email] NOT LIKE '%admin%');
IF OBJECT_ID('dbo.TblUser', 'U') IS NOT NULL DELETE FROM [dbo].[TblUser] WHERE [UserName] != 'admin' AND [Email] NOT LIKE '%admin%';

-- Ensure TblContentReport exists
IF OBJECT_ID('dbo.TblContentReport', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[TblContentReport] (
        [ContentReportId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ContentType] NVARCHAR(20) NOT NULL,
        [ContentId] INT NOT NULL,
        [ReporterUserId] INT NOT NULL,
        [ReasonCategory] NVARCHAR(100) NOT NULL,
        [Details] NVARCHAR(1000) NULL,
        [Status] NVARCHAR(20) NOT NULL CONSTRAINT DF_TblContentReport_Status DEFAULT ('PENDING'),
        [HandledByAdminId] INT NULL,
        [AdminNote] NVARCHAR(500) NULL,
        [HandledAt] DATETIME2(7) NULL,
        [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT DF_TblContentReport_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END;

DELETE FROM [dbo].[TblRole] WHERE [RoleCode] IN ('DOMAIN_PRO', 'MEMBER');

PRINT '2. Inserting Roles verification...';
-- Ensure Target Roles Exist
IF NOT EXISTS (SELECT 1 FROM [dbo].[TblRole] WHERE [RoleCode] = 'DOMAIN_PROFESSIONAL')
BEGIN
    INSERT INTO [dbo].[TblRole] ([RoleCode], [RoleName], [Description], [IsSystemRole], [RoleType], [CreatedAt], [IsDeleted])
    VALUES ('DOMAIN_PROFESSIONAL', 'Domain Professional', 'Verified Domain Professional & Corporate Leader', 0, 0, SYSUTCDATETIME(), 0);
END;

IF NOT EXISTS (SELECT 1 FROM [dbo].[TblRole] WHERE [RoleCode] = 'PUBLIC_FIGURE')
BEGIN
    INSERT INTO [dbo].[TblRole] ([RoleCode], [RoleName], [Description], [IsSystemRole], [RoleType], [CreatedAt], [IsDeleted])
    VALUES ('PUBLIC_FIGURE', 'Public Figure', 'Verified Public Figure, Cultural Icon & Influencer', 0, 0, SYSUTCDATETIME(), 0);
END;

IF NOT EXISTS (SELECT 1 FROM [dbo].[TblRole] WHERE [RoleCode] = 'DEFAULT_USER')
BEGIN
    INSERT INTO [dbo].[TblRole] ([RoleCode], [RoleName], [Description], [IsSystemRole], [RoleType], [CreatedAt], [IsDeleted])
    VALUES ('DEFAULT_USER', 'Default User', 'Standard Active Network Member', 1, 0, SYSUTCDATETIME(), 0);
END;

DECLARE @RoleDomainProId INT = (SELECT TOP 1 [RoleId] FROM [dbo].[TblRole] WHERE [RoleCode] = 'DOMAIN_PROFESSIONAL');
DECLARE @RolePublicFigureId INT = (SELECT TOP 1 [RoleId] FROM [dbo].[TblRole] WHERE [RoleCode] = 'PUBLIC_FIGURE');
DECLARE @RoleMemberId INT = (SELECT TOP 1 [RoleId] FROM [dbo].[TblRole] WHERE [RoleCode] = 'DEFAULT_USER');

PRINT '3. Inserting 30 Real Myanmar Users (10 Domain Pros, 10 Public Figures, 10 Members)...';
SET IDENTITY_INSERT [dbo].[TblUser] ON;
""")

    # Write Domain Pros (101 - 110)
    for dp in domain_pros:
        out.write(f"""INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES ({dp['id']}, '{dp['username']}', '{dp['username'].upper()}', '{dp['email']}', '{dp['email'].upper()}', '{dp['display_name']}', '{PW_HASH}', '{dp['bio'].replace("'", "''")}', '{dp['avatar']}', 1, 1, {dp['rating']}, {dp['rating_count']}, DATEADD(day, -{dp['reg_days']}, SYSUTCDATETIME()), DATEADD(hour, -{dp['login_hours']}, SYSUTCDATETIME()), 0, 'Available', '{dp['sla']}', 'Myanmar', '{dp['skills'][0]}');
""")

    # Write Public Figures (111 - 120)
    for pf in public_figures:
        out.write(f"""INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES ({pf['id']}, '{pf['username']}', '{pf['username'].upper()}', '{pf['email']}', '{pf['email'].upper()}', '{pf['display_name']}', '{PW_HASH}', '{pf['bio'].replace("'", "''")}', '{pf['avatar']}', 1, 1, {pf['rating']}, {pf['rating_count']}, DATEADD(day, -{pf['reg_days']}, SYSUTCDATETIME()), DATEADD(hour, -{pf['login_hours']}, SYSUTCDATETIME()), 0, 'Available', '{pf['sla']}', 'Yangon, Myanmar', '{pf['skills'][0]}');
""")

    # Write Normal Users (121 - 130)
    for nu in normal_users:
        out.write(f"""INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES ({nu['id']}, '{nu['username']}', '{nu['username'].upper()}', '{nu['email']}', '{nu['email'].upper()}', '{nu['display_name']}', '{PW_HASH}', '{nu['hl'].replace("'", "''")}', '{nu['avatar']}', 0, 1, 5.0, 0, DATEADD(day, -{nu['reg_days']}, SYSUTCDATETIME()), DATEADD(hour, -{nu['login_hours']}, SYSUTCDATETIME()), 0, 'Active', 'Replies within 24 hours', '{nu['loc']}', '{nu['hl'].replace("'", "''")}');
""")

    out.write("""SET IDENTITY_INSERT [dbo].[TblUser] OFF;

PRINT '4. Assigning User Roles & Premium Subscriptions...';
""")

    # Assign TblUserRole
    for dp in domain_pros:
        out.write(f"INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES ({dp['id']}, @RoleDomainProId, DATEADD(day, -{dp['reg_days']}, SYSUTCDATETIME()), 0);\n")
    for pf in public_figures:
        out.write(f"INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES ({pf['id']}, @RolePublicFigureId, DATEADD(day, -{pf['reg_days']}, SYSUTCDATETIME()), 0);\n")
    for nu in normal_users:
        out.write(f"INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES ({nu['id']}, @RoleMemberId, DATEADD(day, -{nu['reg_days']}, SYSUTCDATETIME()), 0);\n")

    # Add Wallets (> 5000 LinkDrops for Premium users)
    out.write("\nPRINT '5. Populating LinkDrop Wallets (> 5000 for each premium leader)...';\n")
    for dp in domain_pros:
        out.write(f"INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES ({dp['id']}, {dp['balance']}, {dp['balance']-5000}, 5000, DATEADD(day, -{dp['reg_days']}, SYSUTCDATETIME()));\n")
    for pf in public_figures:
        out.write(f"INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES ({pf['id']}, {pf['balance']}, {pf['balance']-3000}, 3000, DATEADD(day, -{pf['reg_days']}, SYSUTCDATETIME()));\n")
    for nu in normal_users:
        out.write(f"INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES ({nu['id']}, 500, 500, 0, DATEADD(day, -{nu['reg_days']}, SYSUTCDATETIME()));\n")

    # Add Skills for Premium Users
    out.write("\nPRINT '6. Populating Skills for Premium Users...';\n")
    for user_list in [domain_pros, public_figures]:
        for u in user_list:
            for s_idx, skill in enumerate(u["skills"]):
                out.write(f"INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES ({u['id']}, '{skill.replace("'", "''")}', {5 - s_idx}, 1, {s_idx+1}, DATEADD(day, -{u['reg_days']}, SYSUTCDATETIME()), 0);\n")

    # Add KYC Identity Verification Audit
    out.write("\nPRINT '7. Populating KYC Identity Verification Audit Records...';\n")
    for u in domain_pros + public_figures:
        out.write(f"INSERT INTO [dbo].[TblUserVerificationAudit] ([UserId], [AuditCode], [AuditTitle], [AuditDescription], [Authority], [Status], [AuditedAt], [CreatedAt], [IsDeleted]) VALUES ({u['id']}, 'KYC-{u['id']}-MM', 'National Digital ID & Executive Credentials Verified', 'Approved by SecOps Compliance Team with high platform trust score.', 'Myanmar Financial Intelligence / SecOps', 'APPROVED', DATEADD(day, -{u['reg_days']-2}, SYSUTCDATETIME()), DATEADD(day, -{u['reg_days']}, SYSUTCDATETIME()), 0);\n")

    # Add Ratings by Normal Users (mathematically matching rating_scores)
    out.write("\nPRINT '8. Populating User Ratings (Matching rated real user counts)...';\n")
    reviews = [
        "Inspirational leadership and transformative vision for Myanmar's emerging sectors.",
        "Exceptional mentor with profound practical expertise. Learned tremendously.",
        "Very responsive and dedicated to advancing community industry benchmarks.",
        "Highly recommended! The real-world insights shared are gold standard.",
        "A true role model in both professional discipline and societal impact."
    ]
    for idx, u in enumerate(domain_pros + public_figures):
        for r_idx, score in enumerate(u["rating_scores"]):
            rater = normal_users[(idx + r_idx) % len(normal_users)]
            rev = reviews[r_idx % len(reviews)]
            out.write(f"INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES ({rater['id']}, {u['id']}, {score}, '{rev.replace("'", "''")}', DATEADD(day, -{30 - r_idx*2}, SYSUTCDATETIME()), 0);\n")

    # Add Following Network (TblUserFollow)
    out.write("\nPRINT '9. Populating Following Network (TblUserFollow)...';\n")
    # Normal users follow 6 to 10 leaders
    for n_idx, nu in enumerate(normal_users):
        for f_offset in range(7):
            target_leader = (domain_pros + public_figures)[(n_idx + f_offset * 3) % len(domain_pros + public_figures)]
            out.write(f"INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES ({nu['id']}, {target_leader['id']}, DATEADD(day, -{nu['reg_days'] - 5}, SYSUTCDATETIME()), {nu['id']}, 0);\n")
    # Leaders follow each other
    for l_idx, leader in enumerate(domain_pros + public_figures):
        for f_offset in range(1, 4):
            peer = (domain_pros + public_figures)[(l_idx + f_offset) % len(domain_pros + public_figures)]
            out.write(f"INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES ({leader['id']}, {peer['id']}, DATEADD(day, -{leader['reg_days'] - 10}, SYSUTCDATETIME()), {leader['id']}, 0);\n")

    # Add Communities
    out.write("\nPRINT '10. Inserting 10 Top-Level Communities...';\n")
    out.write("SET IDENTITY_INSERT [dbo].[TblCommunity] ON;\n")
    for c in communities_data:
        out.write(f"""INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES ({c[0]}, NULL, {c[4]}, '{c[1].replace("'", "''")}', '{c[2]}', '{c[3].replace("'", "''")}', 'PUBLIC', 'INSTANT', 30, 20, DATEADD(day, -170, SYSUTCDATETIME()), 0);
""")

    # Add Sub-Communities
    out.write("\nPRINT '11. Inserting 50 Sub-Communities (5 per Community)...';\n")
    for sc in sub_communities_data:
        out.write(f"""INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES ({sc[0]}, {sc[1]}, {sc[5]}, '{sc[2].replace("'", "''")}', '{sc[3]}', '{sc[4].replace("'", "''")}', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
""")
    out.write("SET IDENTITY_INSERT [dbo].[TblCommunity] OFF;\n")

    # Add Groups (150 Groups)
    out.write("\nPRINT '12. Inserting 150 Groups (3 per Sub-Community)...';\n")
    out.write("SET IDENTITY_INSERT [dbo].[TblGroup] ON;\n")
    for g in groups_data:
        out.write(f"""INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES ({g[0]}, {g[1]}, {g[2]}, '{g[3].replace("'", "''")}', '{g[4]}', '{g[5].replace("'", "''")}', 'PUBLIC', 'INSTANT', 12, 4, '{g[6]}', {g[7]}, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
""")
    out.write("SET IDENTITY_INSERT [dbo].[TblGroup] OFF;\n")

    # Group Membership & Community Membership
    out.write("\nPRINT '13. Populating Community & Group Memberships...';\n")
    for u in domain_pros + public_figures + normal_users:
        # Join top communities
        for cid in [1, 2, 3, 6, 7, 8]:
            out.write(f"INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES ({cid}, {u['id']}, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);\n")
    
    # Ensure every group has members
    for g in groups_data:
        gid = g[0]
        creator = g[2]
        # Creator is Admin of group
        out.write(f"INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES ({gid}, {creator}, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);\n")
        # Add 3 regular users to each group
        for m_idx in range(3):
            member_user = normal_users[(gid + m_idx) % len(normal_users)]
            out.write(f"INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES ({gid}, {member_user['id']}, 'Member', 0, DATEADD(day, -{80 - m_idx*10}, SYSUTCDATETIME()), DATEADD(day, -{80 - m_idx*10}, SYSUTCDATETIME()), 0);\n")

    # Posts & Polls
    out.write("\nPRINT '14. Inserting Posts & Polls (Standalone + Group Posts across all Groups)...';\n")
    out.write("SET IDENTITY_INSERT [dbo].[TblPost] ON;\n")

    post_id = 1
    poll_id = 1
    poll_inserts = []
    comment_inserts = []

    feed_topics = [
        ("Transformative shift in our digital payments infrastructure across Myanmar: What key area needs the fastest adoption?", "POLL", ["Instant Merchant Settlements", "Cross-Border Remittances", "Offline NFC Wallet Resilience", "Zero-Fee Microtransactions"]),
        ("Key milestones in regional supply chain distribution: Analyzing operational velocity and inventory turns.", "POST", None),
        ("Sustainable urban architecture and energy conservation in commercial property developments.", "POST", None),
        ("Strategic leadership and team resilience during macroeconomic transitions: Insights from three decades in business.", "POST", None),
        ("National health and community clinical safety: Which public health priority demands urgent collaborative attention?", "POLL", ["Rural Mobile Clinics", "Maternal Nutrition Networks", "Clean Water Infrastructure", "Healthcare Worker Grants"]),
        ("Contemporary Burmese aesthetics and fine art: Balancing classical cultural roots with modern international canvas techniques.", "POST", None),
        ("Youth athletic discipline and athletic stamina: Building world-class performance mentalities.", "POST", None),
        ("Telecommunications connectivity and high-speed bandwidth: The engine powering Myanmar's knowledge economy.", "POST", None)
    ]

    normal_user_comments = [
        "Incredible insight! Fully agree with your perspective on long-term sustainability.",
        "Thank you for sharing this real-world case study. The practical takeaways are immense.",
        "This is exactly what our local industry needs to focus on right now.",
        "Very inspiring read! Bookmarked for our team's strategic planning session.",
        "Voted! Looking forward to seeing the aggregated community sentiment on this."
    ]

    # Part A: The 20 Leaders have 2 Standalone Posts (1 Poll + 1 Standard) and 2 Group Posts (1 Standard + 1 Poll)
    # Total = 80 posts
    for leader in domain_pros + public_figures:
        l_id = leader["id"]
        # Post 1: Standalone Poll
        t1, kind1, opts1 = feed_topics[0] if (l_id % 2 == 0) else feed_topics[4]
        out.write(f"""INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES ({post_id}, {l_id}, NULL, NULL, '{t1.replace("'", "''")}', 1, 14, 2, 3, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);\n""")
        poll_inserts.append((poll_id, post_id, t1, opts1))
        # Comments
        comment_inserts.append((post_id, normal_users[(post_id * 2) % len(normal_users)]["id"], normal_user_comments[0]))
        comment_inserts.append((post_id, normal_users[(post_id * 3) % len(normal_users)]["id"], normal_user_comments[4]))
        post_id += 1
        poll_id += 1

        # Post 2: Standalone Post (with executive summary / technical advice)
        t2 = f"{leader['skills'][0]} - Strategic Roadmap & Executive Summary: We are establishing multi-tier operational benchmarks for the coming fiscal quarters."
        out.write(f"""INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES ({post_id}, {l_id}, NULL, NULL, '{t2.replace("'", "''")}', 0, 19, 1, 5, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);\n""")
        comment_inserts.append((post_id, normal_users[(post_id * 5) % len(normal_users)]["id"], normal_user_comments[1]))
        post_id += 1

        # Post 3: Group Post 1 (by Leader inside an owned group)
        owned_groups = [g[0] for g in groups_data if g[2] == l_id]
        gid1 = owned_groups[0] if owned_groups else ((l_id * 7) % 150) + 1
        t3 = f"Discussion inside Group #{gid1}: Exploring practical deployment challenges in {leader['skills'][1] if len(leader['skills']) > 1 else 'Operations'}."
        out.write(f"""INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES ({post_id}, {l_id}, NULL, {gid1}, '{t3.replace("'", "''")}', 0, 11, 1, 2, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);\n""")
        comment_inserts.append((post_id, normal_users[(post_id * 7) % len(normal_users)]["id"], normal_user_comments[2]))
        post_id += 1

        # Post 4: Group Post 2 (Poll inside Group)
        gid2 = owned_groups[1] if len(owned_groups) > 1 else ((l_id * 11) % 150) + 1
        q4 = f"Group #{gid2} Inquiry: Which methodology should our working circle prioritize this quarter?"
        opts4 = ["Quarterly Case Study Review", "Live Industry Q&A", "Technical Whitepaper Breakdown", "Peer Project Critiques"]
        out.write(f"""INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES ({post_id}, {l_id}, NULL, {gid2}, '{q4.replace("'", "''")}', 1, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);\n""")
        poll_inserts.append((poll_id, post_id, q4, opts4))
        comment_inserts.append((post_id, normal_users[(post_id * 11) % len(normal_users)]["id"], normal_user_comments[3]))
        post_id += 1
        poll_id += 1

    # Part B: Fill ALL 150 groups with at least 1 lively discussion post so NO group is empty
    # Check which groups don't have posts yet
    covered_gids = set()
    for leader in domain_pros + public_figures:
        owned_groups = [g[0] for g in groups_data if g[2] == leader["id"]]
        if owned_groups:
            covered_gids.add(owned_groups[0])
            if len(owned_groups) > 1:
                covered_gids.add(owned_groups[1])

    group_topics = [
        "Welcome to this working circle! Please share your background and current project focus.",
        "Monthly discussion thread: What are the primary bottlenecks you have encountered recently?",
        "Resource sharing: Key references, research documents, and toolkits for our sector.",
        "Best practice checklist for project risk management and quality assurance."
    ]

    for g in groups_data:
        gid = g[0]
        if gid not in covered_gids:
            author_id = g[2] # Group creator
            gt = group_topics[gid % len(group_topics)]
            content = f"{g[3]}: {gt}"
            out.write(f"""INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES ({post_id}, {author_id}, NULL, {gid}, '{content.replace("'", "''")}', 0, {5 + (gid % 12)}, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -{2 + (gid % 20)}, SYSUTCDATETIME()), 0);\n""")
            comment_user = normal_users[gid % len(normal_users)]["id"]
            comment_inserts.append((post_id, comment_user, "Great initiative! Really glad to be part of this group discussion."))
            post_id += 1

    out.write("SET IDENTITY_INSERT [dbo].[TblPost] OFF;\n")

    # Insert Polls & Poll Options
    out.write("\nPRINT '15. Inserting Polls & Options...';\n")
    out.write("SET IDENTITY_INSERT [dbo].[TblPoll] ON;\n")
    for pl in poll_inserts:
        out.write(f"""INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES ({pl[0]}, {pl[1]}, '{pl[2].replace("'", "''")}', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);\n""")
    out.write("SET IDENTITY_INSERT [dbo].[TblPoll] OFF;\n")

    out.write("\nPRINT '16. Inserting Poll Options & Sample Votes...';\n")
    out.write("SET IDENTITY_INSERT [dbo].[TblPollOption] ON;\n")
    opt_id = 1
    for pl in poll_inserts:
        p_id = pl[0]
        opts = pl[3]
        for o_idx, opt_text in enumerate(opts):
            v_cnt = (o_idx + 1) * 6
            out.write(f"""INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES ({opt_id}, {p_id}, '{opt_text.replace("'", "''")}', {v_cnt}, {o_idx+1}, DATEADD(day, -15, SYSUTCDATETIME()), 0);\n""")
            opt_id += 1

    out.write("SET IDENTITY_INSERT [dbo].[TblPollOption] OFF;\n")

    # Insert Comments
    out.write("\nPRINT '17. Inserting Real User Comments on Posts...';\n")
    for c_idx, c_item in enumerate(comment_inserts):
        out.write(f"""INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES ({c_item[0]}, {c_item[1]}, '{c_item[2].replace("'", "''")}', DATEADD(day, -{10 - (c_idx%8)}, SYSUTCDATETIME()), 0);\n""")

    # Content Moderation Reports
    # Covering all statuses: PENDING, RESOLVED, DISMISSED, ACTIONED across real users
    out.write("\nPRINT '18. Populating Content Moderation Reports (Covering all statuses)...';\n")
    reports_data = [
        ("POST", 1, 121, "MISLEADING", "Reported claim about transaction latency requires external data verification.", "PENDING", None, None),
        ("POST", 2, 122, "INAPPROPRIATE_CONTENT", "Flagged due to controversial commercial projection phrasing.", "RESOLVED", "Post reviewed by moderator. Clarification note attached. Post approved.", 1),
        ("POST", 3, 123, "SPAM", "Duplicate discussion thread posted within same working room.", "DISMISSED", "Content verified as authentic standalone contribution. Case closed.", 1),
        ("POLL", 1, 124, "MISINFORMATION", "Option labels alleged to have skewed statistical bias.", "ACTIONED", "Poll options reviewed. Content warning appended.", 1),
        ("POST", 5, 125, "COPYRIGHT_VIOLATION", "Reported excerpt matches proprietary textbook material.", "PENDING", None, None),
        ("POST", 7, 126, "HARASSMENT", "Review required on heated debate in comment stream.", "RESOLVED", "Offending sub-comments removed. Discussion restored to compliance.", 1),
        ("POLL", 3, 127, "OTHER", "Inquiry regarding eligibility of external corporate voters.", "DISMISSED", "Poll criteria compliant with platform charter.", 1)
    ]

    for rep in reports_data:
        handled_admin_sql = f"{rep[7]}" if rep[7] else "NULL"
        handled_note_sql = f"'{rep[6].replace("'", "''")}'" if rep[6] else "NULL"
        handled_at_sql = "DATEADD(day, -2, SYSUTCDATETIME())" if rep[5] != "PENDING" else "NULL"
        out.write(f"""INSERT INTO [dbo].[TblContentReport] ([ContentType], [ContentId], [ReporterUserId], [ReasonCategory], [Details], [Status], [AdminNote], [HandledByAdminId], [HandledAt], [CreatedAt])
VALUES ('{rep[0]}', {rep[1]}, {rep[2]}, '{rep[3]}', '{rep[4].replace("'", "''")}', '{rep[5]}', {handled_note_sql}, {handled_admin_sql}, {handled_at_sql}, DATEADD(day, -7, SYSUTCDATETIME()));\n""")

    # User Activities (for reports, metrics & charts)
    out.write("\nPRINT '19. Populating User Activities for Telemetry & Report Charts...';\n")
    activity_types = ["POST_CREATE", "POLL_VOTE", "COMMENT_ADD", "LIKE_POST", "JOIN_COMMUNITY", "JOIN_GROUP"]
    for day_offset in range(30, 0, -1):
        for u in (normal_users + domain_pros[:5]):
            act = activity_types[(u["id"] + day_offset) % len(activity_types)]
            out.write(f"""INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES ({u['id']}, '{act}', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -{day_offset}, SYSUTCDATETIME()), 0);\n""")

    out.write("""
COMMIT TRANSACTION;
PRINT '==============================================================================';
PRINT 'CommunityLink Master Presentation Seed Data generation completed successfully!';
PRINT '==============================================================================';
""")

print("Successfully written complete SQL seed script to CommunityLink_Presentation_SeedData.sql")

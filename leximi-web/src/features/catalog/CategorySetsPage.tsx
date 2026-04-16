import { useParams, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { learningSetService } from '../../api/services';

const CategorySetsPage = () => {
    const { categoryId } = useParams<{ categoryId: string }>();

    const { data: sets, isLoading, error } = useQuery({
        queryKey: ['sets', categoryId],
        queryFn: () => learningSetService.getByCategory(categoryId!),
        enabled: !!categoryId,
    });

    if (isLoading) return <div>Ładowanie zestawów...</div>;
    if (error) return <div>Wystąpił błąd podczas pobierania zestawów.</div>;
    if (!sets || sets.length === 0) return <div>Brak zestawów w tej kategorii.</div>;

    return (
        <div className="sets-container">
            <h1 className="mb-4">Zestawy do nauki</h1>
            <div className="sets-grid">
                {sets.map((set) => (
                    <div key={set.id} className="set-card">
                        <div className="set-card-header">
                            <h3>{set.title}</h3>
                            <div className="set-meta">
                                <span className={`set-type-badge ${set.type?.toLowerCase()}`}>
                                    {set.type}
                                </span>
                            </div>
                        </div>
                        <p>{set.description}</p>
                        <div className="set-card-footer">
                            <Link to={`/sets/${set.id}`} className="button-link">Wybierz</Link>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
};

export default CategorySetsPage;
